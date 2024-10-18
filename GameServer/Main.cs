using System.Collections.Concurrent;
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

Console.WriteLine($"[INFO] Listening on http://{opts.Host}:{opts.Port}/\n");

var listener = new HttpListener();
listener.Prefixes.Add($"http://{opts.Host}:{opts.Port}/");
listener.Start();

opts.Seed ??= new Random().Next();

#if DEBUG
Console.WriteLine("[INFO] Debug mode is enabled.");
#endif

#if HACKATHON
Console.WriteLine("[INFO] Hackathon mode is enabled.");
#endif

#if DEBUG || HACKATHON
Console.WriteLine();
#endif

Console.WriteLine("[INFO] Server started.");
Console.WriteLine("[INFO] Seed: " + opts.Seed);
Console.WriteLine("[INFO] Broadcast interval: " + opts.BroadcastInterval);
Console.WriteLine("[INFO] Ticks: " + opts.Ticks);
Console.WriteLine("[INFO] Join code: " + opts.JoinCode);
Console.WriteLine("[INFO] Number of players: " + opts.NumberOfPlayers);

string? saveReplayPath = null;
if (opts.SaveReplay)
{
    saveReplayPath = opts.ReplayFilepath is not null
        ? Path.GetFullPath(opts.ReplayFilepath)
        : Path.GetFullPath($"Replay_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.json");

    Console.WriteLine("[INFO] Replay will be saved to:");
    Console.WriteLine("[^^^^] " + saveReplayPath);
}

#if HACKATHON
Console.WriteLine("[INFO] Eager broadcast: " + (opts.EagerBroadcast ? "on" : "off"));
#endif

Console.WriteLine("\n[INFO] Press Ctrl+C to stop the server.\n");

var game = saveReplayPath is not null
    ? new GameInstance(opts, saveReplayPath)
    : new GameInstance(opts);

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
    var socket = webSocketContext.WebSocket;

    if (!Enum.TryParse(
        context.Request.QueryString["enumSerializationFormat"] ?? "Int",
        ignoreCase: true,
        out EnumSerializationFormat enumSerialization))
    {
        await RejectConnection(
            new UnknownConnection(context, socket, enumSerialization),
            "InvalidEnumSerializationFormat");
        return;
    }

    var unknownConnection = new UnknownConnection(context, socket, enumSerialization);

#if DEBUG
    Console.WriteLine($"[INFO] Request from {unknownConnection.Ip} ({context.Request.Url})");
#endif

    if (IsIpBlocked(unknownConnection.Ip))
    {
        await RejectConnection(unknownConnection, "TooManyFailedAttempts");
        return;
    }

    string? joinCode = context.Request.QueryString["joinCode"];

    if (!IsJoinCodeValid(joinCode))
    {
        RegisterFailedAttempt(context.Request.RemoteEndPoint.Address.ToString());
        await RejectConnection(unknownConnection, "InvalidJoinCode");
        return;
    }

    if (absolutePath.Equals("/spectator", StringComparison.OrdinalIgnoreCase))
    {
        await await HandleSpectatorConnection(context, socket, unknownConnection);
    }
    else if (absolutePath.Equals("/") || string.IsNullOrEmpty(absolutePath))
    {
        await await HandlePlayerConnection(context, socket, unknownConnection);
    }
    else
    {
        unknownConnection.TargetType = absolutePath[1..];
        await RejectConnection(unknownConnection, "InvalidUrlPath");
    }
}

async Task<Task> HandlePlayerConnection(
    HttpListenerContext context,
    WebSocket socket,
    UnknownConnection unknownConnection)
{
    unknownConnection.TargetType = "Player";

    string? nickname = context.Request.QueryString["nickname"]?.ToUpper();

    if (string.IsNullOrEmpty(nickname))
    {
        return RejectConnection(unknownConnection, "MissingNickname");
    }

    string? playerType = context.Request.QueryString["playerType"];

    if (string.IsNullOrEmpty(playerType))
    {
        return RejectConnection(unknownConnection, "MissingPlayerType");
    }

    if (!Enum.TryParse(playerType, ignoreCase: true, out PlayerType type))
    {
        return RejectConnection(unknownConnection, "InvalidPlayerType");
    }

#if DEBUG
    _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#endif

    GameLogic.Player player;
    PlayerConnection connection;

    lock (game.PlayerManager)
    {
        if (game.Players.Count() >= opts.NumberOfPlayers)
        {
            return RejectConnection(unknownConnection, "GameFull");
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
                return RejectConnection(unknownConnection, "NicknameExists");
            }
        }

        var connectionData = new ConnectionData.Player(nickname, type, unknownConnection.EnumSerialization)
#if DEBUG
        {
            QuickJoin = quickJoin,
        }
#endif
        ;

        player = game.PlayerManager.CreatePlayer(connectionData);
        connection = new PlayerConnection(context, socket, connectionData, player);
        game.AddConnection(connection);
    }

    await AcceptConnection(connection);
    _ = Task.Run(() => game.PacketHandler.HandleConnection(connection));

    var pingCts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token);
    _ = Task.Run(() => PingClientLoop(connection, pingCts.Token), pingCts.Token);

    if (game.GameManager.Status is GameStatus.InLobby)
    {
        _ = Task.Run(game.LobbyManager.SendLobbyDataToAll);
    }

#if DEBUG
    if (quickJoin)
    {
        game.GameManager.StartGame();
        await game.LobbyManager.SendLobbyDataTo(connection);
    }
#endif

    return Task.CompletedTask;
}

async Task<Task> HandleSpectatorConnection(
    HttpListenerContext context,
    WebSocket socket,
    UnknownConnection unknownConnection)
{
    unknownConnection.TargetType = "Spectator";

    string? joinCode = context.Request.QueryString["joinCode"];

#if DEBUG
    _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#endif

    var connectionData = new ConnectionData(unknownConnection.EnumSerialization)
#if DEBUG
    {
        QuickJoin = quickJoin,
    }
#endif
    ;

    var connection = new SpectatorConnection(context, socket, connectionData);
    await AcceptConnection(connection);
    game.AddConnection(connection);
    _ = Task.Run(() => game.PacketHandler.HandleConnection(connection));

    var pingCts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token);
    _ = Task.Run(() => PingClientLoop(connection, pingCts.Token), pingCts.Token);

#if DEBUG
    if (quickJoin)
    {
        game.GameManager.StartGame();
    }

    // A temporary solution allowing the client to change the game scene
    await Task.Delay(500);
#endif

    await game.LobbyManager.SendLobbyDataTo(connection);

    return Task.CompletedTask;
}

bool IsJoinCodeValid(string? joinCode)
{
    return joinCode == opts.JoinCode;
}

bool NicknameAlreadyExists(string nickname)
{
    return game.Players.Any(p => p.Instance.Nickname == nickname);
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

async Task AcceptConnection(Connection connection)
{
    var payload = new EmptyPayload() { Type = PacketType.ConnectionAccepted };
    var packet = new ResponsePacket(payload);
    await packet.SendAsync(connection);
}

async Task RejectConnection(Connection connection, string reason)
{
    Console.WriteLine("[INFO] Connection rejected.");
    Console.WriteLine("[^^^^] Client: {0}", connection);
    Console.WriteLine("[^^^^] Reason: {0}", reason);

    var payload = new ConnectionRejectedPayload(reason);
    var packet = new ResponsePacket(payload);
    await packet.SendAsync(connection);
    await connection.CloseAsync(description: reason);
}

/// <summary>
/// Pings a player in a loop.
/// </summary>
/// <param name="socket">The socket of the player.</param>
/// <param name="cancellationToken">The cancellation token that can be used to cancel the ping loop.</param>
/// <returns>A task representing the asynchronous operation.</returns>
async Task PingClientLoop(Connection connection, CancellationToken cancellationToken)
{
    const int pingInterval = 1000;

    while (!cancellationToken.IsCancellationRequested && connection.Socket.State == WebSocketState.Open)
    {
        try
        {
            var payload = new EmptyPayload() { Type = PacketType.Ping };
            var packet = new ResponsePacket(payload);
            await packet.SendAsync(connection);
            connection.LastPingSentTime = DateTime.UtcNow;
            await Task.Delay(pingInterval, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[WARN] Failed to ping client.");
            Console.WriteLine("[^^^^] Client: {0}", connection);
            Console.WriteLine("[^^^^] Exception: {0}", ex.Message);

            // A small delay to prevent tight loop in case of persistent errors
            await Task.Delay(100, cancellationToken);
        }
    }
}
