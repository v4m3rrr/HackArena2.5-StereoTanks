using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
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

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();
    string? joinCode = context.Request.QueryString["joinCode"];
    string absolutePath = context.Request.Url?.AbsolutePath ?? string.Empty;
    string clientIP = context.Request.RemoteEndPoint.Address.ToString();

    string connType = context.Request.IsWebSocketRequest ? "WebSocket" : "HTTP";

    Console.WriteLine($"Request ({connType}) from {clientIP} for {absolutePath}");

    bool isSpectator = false;
    if (absolutePath.Equals("/spectator", StringComparison.OrdinalIgnoreCase))
    {
        isSpectator = true;
    }
    else if (absolutePath.Equals("/", StringComparison.OrdinalIgnoreCase))
    {
        isSpectator = false;
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        byte[] buffer = Encoding.UTF8.GetBytes("Invalid path");
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
        continue;
    }

    if (IsIpBlocked(clientIP))
    {
        Console.WriteLine($"IP {clientIP} is temporarily blocked.");
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        byte[] buffer = Encoding.UTF8.GetBytes("Too many attempts. Try again later.");
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
        continue;
    }

    if (!IsJoinCodeValid(joinCode))
    {
        RegisterFailedAttempt(clientIP);
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        byte[] buffer = Encoding.UTF8.GetBytes("Invalid join code");
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
        continue;
    }

    if (!context.Request.IsWebSocketRequest)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.Close();
        continue;
    }

    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
    WebSocket webSocket = webSocketContext.WebSocket;

    Action<WebSocket> addClientMethod = isSpectator ? game.AddSpectator : game.AddPlayer;
    addClientMethod(webSocket);

    _ = Task.Run(() => game.HandleConnection(webSocket, clientIP));
    _ = Task.Run(() => game.PingClientLoop(webSocket));
}

bool IsJoinCodeValid(string? joinCode)
{
    return joinCode == opts.JoinCode;
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
