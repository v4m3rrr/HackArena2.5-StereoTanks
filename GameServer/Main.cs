using System.Collections.Concurrent;
using System.Diagnostics;
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
game.Grid.GenerateWalls();

var failedAttempts = new ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)>();

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();

    string? joinCode = context.Request.QueryString["joinCode"];
    string clientIP = context.Request.RemoteEndPoint.Address.ToString();

    var connType = context.Request.IsWebSocketRequest ? "WebSocket" : "HTTP";
    Console.WriteLine($"Received ({connType}) join code from {clientIP}: {joinCode}");

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

    game.AddClient(webSocket);

    _ = Task.Run(() => game.HandleConnection(webSocket));
    _ = Task.Run(() => game.SendGameData(webSocket));
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
