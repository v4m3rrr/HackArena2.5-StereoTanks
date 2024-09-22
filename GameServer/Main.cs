using System.Collections.Concurrent;
using System.Net;
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
Console.WriteLine("Join code: " + opts.JoinCode);
Console.WriteLine("Number of players: " + opts.NumberOfPlayers);
Console.WriteLine("Eager broadcast: " + (opts.EagerBroadcast ? "on" : "off"));

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

    Console.Write($"Request ({(context.Request.IsWebSocketRequest ? "WebSocket" : "HTTP")}) ");
    Console.WriteLine($"from {clientIP} for {context.Request.Url?.AbsolutePath}");

    if (IsIpBlocked(clientIP))
    {
        await RespondWithError(context, HttpStatusCode.TooManyRequests, "Too many attempts. Try again later.");
        return;
    }

    if (absolutePath.Equals("/spectator", StringComparison.OrdinalIgnoreCase))
    {
        await HandleSpectatorConnection(context);
    }
    else if (absolutePath.Equals("/") || string.IsNullOrEmpty(absolutePath))
    {
        await HandlePlayerConnection(context);
    }
    else
    {
        await RespondWithError(context, HttpStatusCode.NotFound, "Invalid path");
    }
}

async Task HandlePlayerConnection(HttpListenerContext context)
{
    string? joinCode = context.Request.QueryString["joinCode"];
    string? nickname = context.Request.QueryString["nickname"]?.ToUpper();

#if DEBUG
    _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#endif

    if (!IsJoinCodeValid(joinCode))
    {
        RegisterFailedAttempt(context.Request.RemoteEndPoint.Address.ToString());
        await RespondWithError(context, HttpStatusCode.Unauthorized, "Invalid join code");
        return;
    }

    if (!Enum.TryParse(
        context.Request.QueryString["typeOfPacketType"] ?? "Int",
        ignoreCase: true,
        out TypeOfPacketType typeOfPacketType))
    {
        await RespondWithError(context, HttpStatusCode.BadRequest, "Invalid type of packet type");
        return;
    }

    if (string.IsNullOrEmpty(nickname))
    {
        await RespondWithError(context, HttpStatusCode.BadRequest, "Nickname is required");
        return;
    }


    if (game.PlayerManager.Players.Count >= opts.NumberOfPlayers)
    {
        await RespondWithError(context, HttpStatusCode.Forbidden, "Game is full");
        return;
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
            await RespondWithError(context, HttpStatusCode.Conflict, "Nickname already exists");
            return;
        }
    }

    if (!context.Request.IsWebSocketRequest)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.Close();
        return;
    }

    var webSocketContext = await context.AcceptWebSocketAsync(null);
    var webSocket = webSocketContext.WebSocket;

#if DEBUG
    var playerConnectionData = new PlayerConnectionData(nickname, typeOfPacketType, quickJoin);
#else
    var playerConnectionData = new PlayerConnectionData(nickname, typeOfPacketType);
#endif

    var player = game.PlayerManager.AddPlayer(webSocket, playerConnectionData);

    game.HandleConnection(webSocket);

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
}

async Task HandleSpectatorConnection(HttpListenerContext context)
{
    string? joinCode = context.Request.QueryString["joinCode"];

#if DEBUG
    _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#endif

    if (!IsJoinCodeValid(joinCode))
    {
        RegisterFailedAttempt(context.Request.RemoteEndPoint.Address.ToString());
        await RespondWithError(context, HttpStatusCode.Unauthorized, "Invalid join code");
        return;
    }

    if (!context.Request.IsWebSocketRequest)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.Close();
        return;
    }

    var webSocketContext = await context.AcceptWebSocketAsync(null);
    var webSocket = webSocketContext.WebSocket;

    game.SpectatorManager.AddSpectator(webSocket);
    game.HandleConnection(webSocket);

    _ = Task.Run(() => game.LobbyManager.SendLobbyDataToSpectator(webSocket));

#if DEBUG
    if (quickJoin)
    {
        game.GameManager.StartGame();
    }
#endif
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

async Task RespondWithError(HttpListenerContext context, HttpStatusCode statusCode, string message)
{
    context.Response.StatusCode = (int)statusCode;
    byte[] buffer = Encoding.UTF8.GetBytes(message);
    context.Response.ContentLength64 = buffer.Length;
    await context.Response.OutputStream.WriteAsync(buffer);
    context.Response.Close();
}
