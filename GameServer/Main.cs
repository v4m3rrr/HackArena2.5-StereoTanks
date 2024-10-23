using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using GameLogic.Networking;
using GameServer;
using Serilog;

string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ffff");
var log = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss:ffff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File($"logs/{timestamp}.log")
    .CreateLogger();

var assembly = Assembly.GetExecutingAssembly();
var version = assembly.GetName().Version!;
var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;

#if WINDOWS
var platform = "Windows";
#elif LINUX
var platform = "Linux";
#elif OSX
var platform = "macOS";
#else
var platform = "Unknown";
#endif

var sb = new StringBuilder()
    .Append('v')
    .Append(version.Major)
    .Append('.')
    .Append(version.Minor)
    .Append('.')
    .Append(version.Build)
    .Append('.')
    .Append(version.Revision)
    .Append(" (")
    .Append(platform)
    .Append(')');

log.Information("GameServer {version}", sb);

CommandLineOptions? opts = CommandLineParser.Parse(args, log);

if (opts is null)
{
    return;
}

log.Information("Listening on http://{host}:{port}/\n", opts.Host, opts.Port);

var listener = new HttpListener();
listener.Prefixes.Add($"http://{opts.Host}:{opts.Port}/");
listener.Start();

opts.Seed ??= new Random().Next();

#if DEBUG
log.Information("Debug mode is enabled.");
#endif

#if HACKATHON
log.Information("Hackathon mode is enabled.");
#endif

#if DEBUG || HACKATHON
Console.WriteLine();
#endif

log.Information("Server started.");
log.Information("Seed: {seed}", opts.Seed);
log.Information("Broadcast interval: {interval}", opts.BroadcastInterval);
log.Information("Ticks: {ticks}", opts.SandboxMode ? "n/a" : opts.Ticks);
log.Information("Join code: {code}", opts.JoinCode ?? string.Empty);
log.Information("Number of players: {number}", opts.NumberOfPlayers);
log.Information("Sandbox mode: {mode}", opts.SandboxMode ? "on" : "off");

string? saveReplayPath = null;
if (opts.SaveReplay)
{
    saveReplayPath = opts.ReplayFilepath is not null
        ? Path.GetFullPath(opts.ReplayFilepath)
        : Path.GetFullPath($"Replay_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.json");

    log.Information("Replay will be saved to: {path}", saveReplayPath);
}

#if HACKATHON
log.Information("Eager broadcast: {status}", opts.EagerBroadcast ? "on" : "off");
#endif

Console.WriteLine();

log.Information("Press Ctrl+C to stop the server.\n");

var game = saveReplayPath is not null
    ? new GameInstance(opts, log, saveReplayPath)
    : new GameInstance(opts, log);

log.Information("Generating map...");
game.Grid.GenerationWarning += (s, e) => log.Warning("Map generation: {e}", e);
game.Grid.GenerateMap();
log.Information("Map generated.");

if (opts.SandboxMode)
{
    game.GameManager.StartGame();
}

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
            new UnknownConnection(context, socket, enumSerialization, log),
            "InvalidEnumSerializationFormat");
        return;
    }

    var unknownConnection = new UnknownConnection(context, socket, enumSerialization, log);
    log.Debug("Request from {ip} ({url})", unknownConnection.Ip, context.Request.Url);

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
#else
    bool quickJoin = false;
#endif

    GameLogic.Player player;
    PlayerConnection connection;

    if (!quickJoin && !opts.SandboxMode)
    {
        if (game.GameManager.IsInProgess)
        {
            return RejectConnection(unknownConnection, "GameInProgress");
        }
    }

    lock (game.GameManager)
    {
        if (!quickJoin && game.Players.Count() >= opts.NumberOfPlayers)
        {
            return RejectConnection(unknownConnection, "GameFull");
        }

        if (NicknameAlreadyExists(nickname))
        {
            if (quickJoin || opts.SandboxMode)
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

        lock (game.GameManager)
        {
            player = game.PlayerManager.CreatePlayer(connectionData);
            connection = new PlayerConnection(context, socket, connectionData, log, player);
            game.AddConnection(connection);
        }
    }

    await AcceptConnection(connection);
    _ = Task.Run(() => game.PacketHandler.HandleConnection(connection));

    var pingCts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token);
    _ = Task.Run(() => PingClientLoop(connection, pingCts.Token), pingCts.Token);

    if (game.GameManager.Status is GameStatus.InLobby)
    {
        _ = Task.Run(game.LobbyManager.SendLobbyDataToAll);
    }

    if (quickJoin)
    {
        game.GameManager.StartGame();
        await game.LobbyManager.SendLobbyDataTo(connection);
    }

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
#else
    bool quickJoin = false;
#endif

    var connectionData = new ConnectionData(unknownConnection.EnumSerialization)
#if DEBUG
    {
        QuickJoin = quickJoin,
    }
#endif
    ;

    var connection = new SpectatorConnection(context, socket, connectionData, log);
    await AcceptConnection(connection);
    game.AddConnection(connection);
    _ = Task.Run(() => game.PacketHandler.HandleConnection(connection));

    var pingCts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token);
    _ = Task.Run(() => PingClientLoop(connection, pingCts.Token), pingCts.Token);

    await game.LobbyManager.SendLobbyDataTo(connection);

    if (quickJoin)
    {
        game.GameManager.StartGame();
    }
    else
    {
        if (game.GameManager.IsInProgess)
        {
            await game.LobbyManager.SendGameStartedTo(connection);
        }
    }

    return Task.CompletedTask;
}

bool IsJoinCodeValid(string? joinCode)
{
    return joinCode?.ToLower() == opts.JoinCode?.ToLower();
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
    int attemptNumber = failedAttempts.TryGetValue(clientIP, out var attemptInfo)
        ? attemptInfo.Attempts + 1
        : 1;

    log.Verbose(
        "Failed attempt ({attemptNumber}) from {ip}",
        attemptNumber,
        clientIP);

    _ = failedAttempts.AddOrUpdate(
        clientIP,
        (1, DateTime.Now),
        (key, oldValue) => (oldValue.Attempts + 1, DateTime.Now));
}

async Task AcceptConnection(Connection connection)
{
    var payload = new EmptyPayload() { Type = PacketType.ConnectionAccepted };
    var packet = new ResponsePacket(payload, log);
    await packet.SendAsync(connection);
}

async Task RejectConnection(Connection connection, string reason)
{
    log.Information("Connection rejected; {connection}; {reason}", connection, reason);

    var payload = new ConnectionRejectedPayload(reason);
    var packet = new ResponsePacket(payload, log);
    await packet.SendAsync(connection);
    await connection.CloseAsync(description: reason);
}

/// <summary>
/// Pings a client in a loop.
/// </summary>
/// <param name="connection">The connection to ping.</param>
/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
/// <returns>A task representing the asynchronous operation.</returns>
async Task PingClientLoop(Connection connection, CancellationToken cancellationToken)
{
    const int pingInterval = 1000;
    const int pongTimeout = 10000;

    await Task.Delay(500, cancellationToken);

    void SendPing()
    {
        log.Verbose("Sending ping to {connection}", connection);

        var payload = new EmptyPayload() { Type = PacketType.Ping };
        var packet = new ResponsePacket(payload, log);

        var cancellationToken = new CancellationTokenSource(pongTimeout).Token;
        _ = packet.SendAsync(connection, cancellationToken);
    }

    SendPing();
    connection.LastPingSentTime = DateTime.UtcNow;

    while (!cancellationToken.IsCancellationRequested && connection.Socket.State == WebSocketState.Open)
    {
        try
        {
            if (connection.HasSentPong)
            {
                connection.HasSentPong = false;
                SendPing();
                connection.LastPingSentTime = DateTime.UtcNow;
            }
            else
            {
                var timeout = pongTimeout * (connection.IsSecondPingAttempt ? 2 : 1);
                if ((DateTime.UtcNow - connection.LastPingSentTime).TotalMilliseconds > timeout)
                {
                    if (!connection.IsSecondPingAttempt)
                    {
                        log.Warning("Client did not respond pong in {time}ms! Sending second ping... ({connection})", pongTimeout, connection);

                        SendPing();
                        connection.IsSecondPingAttempt = true;
                    }
                    else
                    {
                        log.Warning("No pong response after second ping, disconnecting... ({connection})", connection);

                        var token = new CancellationTokenSource(1000).Token;

                        try
                        {
                            _ = connection.CloseAsync(description: "NoPongResponse", cancellationToken: token);
                        }
                        catch (OperationCanceledException)
                        {
                            log.Error("Failed to close connection normally, aborting... ({connection}))", connection);
                            connection.Socket.Abort();
                        }

                        game.RemoveConnection(connection.Socket);
                        return;
                    }
                }
            }

            await Task.Delay(pingInterval, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            log.Debug("Ping loop canceled. ({connection})", connection);
            break;
        }
        catch (Exception ex)
        {
            log.Warning(ex, "Failed to ping client. ({connection})", connection);

            // A small delay to prevent tight loop in case of persistent errors
            log.Verbose("Waiting for 100ms before retrying... ({connection})", connection);
            await Task.Delay(100, cancellationToken);
        }
    }
}
