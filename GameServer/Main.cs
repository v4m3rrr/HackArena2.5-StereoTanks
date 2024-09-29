using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using GameLogic.Networking;
using GameServer;

CommandLineOptions? opts = CommandLineParser.Parse(args);

if (opts is null)
{
    return;
}

Console.WriteLine($"Listening on http://{opts.Host}:{opts.Port}/");

var listener = new HttpListener();
listener.Prefixes.Add($"http://{opts.Host}:{opts.Port}/");
listener.Start();

opts.Seed ??= new Random().Next();

Console.WriteLine("Server started.");
Console.WriteLine("Seed: " + opts.Seed);
Console.WriteLine("Broadcast interval: " + opts.BroadcastInterval);
Console.WriteLine("Ticks: " + opts.Ticks);
Console.WriteLine("Join code: " + opts.JoinCode);
Console.WriteLine("Number of players: " + opts.NumberOfPlayers);

#if HACKATHON
Console.WriteLine("Eager broadcast: " + (opts.EagerBroadcast ? "on" : "off"));
#endif

Console.WriteLine("\nPress Ctrl+C to stop the server.\n");

var game = new GameInstance(opts);
game.Grid.GenerateMap();

var failedAttempts = new ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)>();

var serverCts = new CancellationTokenSource();

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();
    _ = Task.Run(() => HandleRequest(context));
}

async Task HandleRequest(HttpListenerContext context)
{
    string clientIP = context.Request.RemoteEndPoint.Address.ToString();
    string absolutePath = context.Request.Url?.AbsolutePath ?? string.Empty;

    if (!context.Request.IsWebSocketRequest)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        byte[] buffer = Encoding.UTF8.GetBytes("WebSocket is required");
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
        return;
    }

    var webSocketContext = await context.AcceptWebSocketAsync(null);
    var webSocket = webSocketContext.WebSocket;

    Console.WriteLine($"Request from {clientIP} ({context.Request.Url})");

    if (IsIpBlocked(clientIP))
    {
        await RejectConnection(context, webSocket, "Too many failed attempts. Try again later.");
        return;
    }

    string? joinCode = context.Request.QueryString["joinCode"];

    if (!IsJoinCodeValid(joinCode))
    {
        RegisterFailedAttempt(context.Request.RemoteEndPoint.Address.ToString());
        await RejectConnection(context, webSocket, "Invalid join code");
        return;
    }

    if (absolutePath.Equals("/spectator", StringComparison.OrdinalIgnoreCase))
    {
        await await HandleSpectatorConnection(context, webSocket);
    }
    else if (absolutePath.Equals("/") || string.IsNullOrEmpty(absolutePath))
    {
        await await HandlePlayerConnection(context, webSocket);
    }
    else
    {
        await RejectConnection(context, webSocket, "Invalid path");
    }
}

async Task<Task> HandlePlayerConnection(HttpListenerContext context, WebSocket webSocket)
{
    if (!Enum.TryParse(
        context.Request.QueryString["typeOfPacketType"] ?? "Int",
        ignoreCase: true,
        out TypeOfPacketType typeOfPacketType))
    {
        return RejectConnection(context, webSocket, "Invalid type of packet type");
    }

    string? nickname = context.Request.QueryString["nickname"]?.ToUpper();

    if (string.IsNullOrEmpty(nickname))
    {
        return RejectConnection(context, webSocket, "Nickname is required");
    }

    string? playerType = context.Request.QueryString["playerType"];

    if (string.IsNullOrEmpty(playerType))
    {
        return RejectConnection(context, webSocket, "Player type is required");
    }

    if (!Enum.TryParse(playerType, ignoreCase: true, out PlayerType type))
    {
        return RejectConnection(context, webSocket, "Invalid player type");
    }

#if DEBUG
    _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#endif

    GameLogic.Player player;

    lock (game.PlayerManager.Players)
    {
        if (game.PlayerManager.Players.Count >= opts.NumberOfPlayers)
        {
            return RejectConnection(context, webSocket, "Game is full");
        }

        if (NicknameAlreadyExists(nickname))
        {
#if DEBUG
            if (quickJoin)
            {
                int i = 0;
                string newNickname = nickname;
                while (NicknameAlreadyExists(newNickname))
                {
                    newNickname = $"{nickname}{++i}";
                }

                nickname = newNickname;
            }
            else
#endif
            {
                return RejectConnection(context, webSocket, "Nickname already exists");
            }
        }

#if DEBUG
        var playerConnectionData = new PlayerConnectionData(nickname, type, typeOfPacketType, quickJoin);
#else
        var playerConnectionData = new PlayerConnectionData(nickname, type, typeOfPacketType);
#endif

        player = game.PlayerManager.AddPlayer(webSocket, playerConnectionData);
    }

    game.HandleConnection(webSocket);
    await AcceptConnection(context, webSocket);

    var playerCts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token);
    _ = Task.Run(() => game.PlayerManager.PingPlayerLoop(webSocket, playerCts.Token), playerCts.Token);

    if (game.GameManager.Status is GameStatus.InLobby)
    {
        _ = Task.Run(game.LobbyManager.SendLobbyDataToAll);
    }

#if DEBUG
    if (quickJoin)
    {
        game.GameManager.StartGame();
        _ = Task.Run(() => game.LobbyManager.SendLobbyDataToPlayer(webSocket, player.Id));
    }
#endif

    return Task.CompletedTask;
}

async Task<Task> HandleSpectatorConnection(HttpListenerContext context, WebSocket webSocket)
{
    string? joinCode = context.Request.QueryString["joinCode"];

#if DEBUG
    _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#endif

    game.SpectatorManager.AddSpectator(webSocket);
    game.HandleConnection(webSocket);
    await AcceptConnection(context, webSocket);

#if DEBUG
    if (quickJoin)
    {
        game.GameManager.StartGame();
    }

    // A temporary solution allowing the client to change the game scene
    await Task.Delay(500);
#endif

    await game.LobbyManager.SendLobbyDataToSpectator(webSocket);

    return Task.CompletedTask;
}

bool IsJoinCodeValid(string? joinCode)
{
    return joinCode == opts.JoinCode;
}

bool NicknameAlreadyExists(string nickname)
{
    return game.PlayerManager.Players.Values.Any(p => p.Instance.Nickname == nickname);
}

bool IsIpBlocked(string clientIP)
{
    if (failedAttempts.TryGetValue(clientIP, out var attemptInfo))
    {
        if (attemptInfo.Attempts >= 5 && (DateTime.Now - attemptInfo.LastAttempt).TotalMinutes < 15)
        {
            return true;
        }

        if ((DateTime.Now - attemptInfo.LastAttempt).TotalMinutes >= 15)
        {
            _ = failedAttempts.TryRemove(clientIP, out _);
        }
    }

    return false;
}

void RegisterFailedAttempt(string clientIP)
{
    _ = failedAttempts.AddOrUpdate(
        clientIP,
        (1, DateTime.Now),
        (key, oldValue) => (oldValue.Attempts + 1, DateTime.Now));
}

async Task AcceptConnection(HttpListenerContext context, WebSocket socket)
{
    var typeOfPacketType = GetTypeOfPacketTypeFromQueryString(context.Request.QueryString);
    var payload = new EmptyPayload() { Type = PacketType.ConnectionAccepted };
    var options = new SerializationOptions() { TypeOfPacketType = typeOfPacketType };
    var buffer = PacketSerializer.ToByteArray(payload, [], options);

    Console.WriteLine($"Connection from {context.Request.RemoteEndPoint.Address} accepted.");

    Monitor.Enter(socket);

    try
    {
        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    finally
    {
        Monitor.Exit(socket);
    }
}

async Task RejectConnection(HttpListenerContext context, WebSocket socket, string message)
{
    var typeOfPacketType = GetTypeOfPacketTypeFromQueryString(context.Request.QueryString);
    var payload = new ConnectionRejectedPayload(message);
    var options = new SerializationOptions() { TypeOfPacketType = typeOfPacketType };
    var buffer = PacketSerializer.ToByteArray(payload, [], options);

    Console.WriteLine($"Connection from {context.Request.RemoteEndPoint.Address} rejected: {message}.");

    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
}

TypeOfPacketType GetTypeOfPacketTypeFromQueryString(NameValueCollection queryString)
{
    return Enum.TryParse(queryString["typeOfPacketType"], ignoreCase: true, out TypeOfPacketType t)
        ? t : TypeOfPacketType.Int;
}
