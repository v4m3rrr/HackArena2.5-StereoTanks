using System.Net;
using System.Net.WebSockets;
using GameServer;

GameManager gameManager = new GameManager();

HttpListener listener = new HttpListener();
listener.Prefixes.Add("http://*:5000/");
listener.Start();
Console.WriteLine("Server started.");

_ = Task.Run(BroadcastLoop);

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();
    if (context.Request.IsWebSocketRequest)
    {
        string? joinCode = context.Request.QueryString["joinCode"];
        if (string.IsNullOrEmpty(joinCode))
        {
            var gameInstance = gameManager.CreateGame();
            joinCode = gameInstance.JoinCode;
        }

        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
        WebSocket webSocket = webSocketContext.WebSocket;
        var game = gameManager.GetGameByJoinCode(joinCode);
        if (game == null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid join code", CancellationToken.None);
            continue;
        }

        game.AddClient(webSocket);
        _ = Task.Run(() => HandleConnection(game, webSocket));
    }
}

async Task HandleConnection(GameInstance game, WebSocket socket)
{
    var cancellationTokenSource = new CancellationTokenSource();

    while (socket.State == WebSocketState.Open)
    {
        var buffer = new byte[1024 * 32];
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
        if (result.MessageType == WebSocketMessageType.Text)
        {
            _ = game.HandleBuffer(socket, buffer);
        }
        else if (result.MessageType == WebSocketMessageType.Close)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
            game.RemoveClient(socket);
        }
    }
}

async Task BroadcastLoop()
{
    while (true)
    {
        var startTime = DateTime.UtcNow;
        foreach (var game in gameManager.GetAllGames())
        {
            game.Grid.UpdateBullets(1f);
            await game.BroadcastGridState();
        }

        // TODO: Delay should be called within each game instance
        await Task.Delay(Math.Max(0, (int)(GameInstance.BroadcastInterval * 1000) - (int)(DateTime.UtcNow - startTime).TotalMilliseconds));
    }
}
