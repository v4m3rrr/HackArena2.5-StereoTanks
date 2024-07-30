using System.Net.WebSockets;
using GameLogic;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents a game instance.
/// </summary>
internal class GameInstance
{
    private static readonly Random Random = new();
    private static readonly IEnumerable<PacketType> MovementRestrictedPacketTypes = [
        PacketType.TankMovement,
        PacketType.TankRotation,
        PacketType.TankShoot,
    ];

    private readonly Dictionary<WebSocket, DateTime> lastActionTime = [];
    private readonly Dictionary<WebSocket, Player> players = [];
    private readonly List<Player> playersWhoSentMovementPacketThisTick = [];

    private readonly Dictionary<Player, DateTime> pingSend = [];
    private readonly Dictionary<Player, bool> pingReceived = [];

    private readonly int broadcastInterval;
    private readonly bool eagerBroadcast;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    public GameInstance(CommandLineOptions options)
    {
        int seed = options.Seed!.Value;
        this.Grid = new Grid(seed);

        this.broadcastInterval = options.BroadcastInterval;
        this.eagerBroadcast = options.EagerBroadcast;

        _ = Task.Run(this.GameStateBroadcastLoop);
    }

    private event Action<Player>? PlayerSentMovementPacket;

    /// <summary>
    /// Gets the grid of the game.
    /// </summary>
    public Grid Grid { get; private set; }

    /// <summary>
    /// Adds a client to the game.
    /// </summary>
    /// <param name="socket">The socket of the client to add.</param>
    public void AddClient(WebSocket socket)
    {
        var color = (uint)(255 << 24 | (uint)Random.Next(0xFFFFFF));

        string id;
        do
        {
            id = Guid.NewGuid().ToString();
        } while (this.players.Values.Any(p => p.Id == id));

        var player = new Player(id, $"Player {this.players.Count + 1}", color);

        _ = this.Grid.GenerateTank(player);
        this.players.Add(socket, player);
        this.lastActionTime.Add(socket, DateTime.MinValue);
    }

    /// <summary>
    /// Removes a client from the game.
    /// </summary>
    /// <param name="socket">The socket of the client to remove.</param>
    public void RemoveClient(WebSocket socket)
    {
        var player = this.players.Remove(socket);
        /* Grid.RemoveTank(player.Tank); TODO */
        _ = this.lastActionTime.Remove(socket);
    }

    /// <summary>
    /// Handles a connection.
    /// </summary>
    /// <param name="socket">The socket of the client to handle.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleConnection(WebSocket socket)
    {
        var cancellationTokenSource = new CancellationTokenSource();

        while (socket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 32];
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                await this.HandleBuffer(socket, buffer);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                this.RemoveClient(socket);
            }
        }
    }

    /// <summary>
    /// Sends the game data to a client.
    /// </summary>
    /// <param name="socket">The socket of the client to send the game data to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendGameData(WebSocket socket)
    {
        var response = new GameDataPayload(
            this.players[socket].Id,
            this.Grid.Dim,
            this.Grid.Seed,
            this.broadcastInterval);
        await SendPacketAsync(socket, response);
    }

    /// <summary>
    /// Pings the client.
    /// </summary>
    /// <param name="socket">The socket of the client to ping.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task PingClientLoop(WebSocket socket)
    {
        while (true)
        {
            var player = this.players[socket];
            if (this.pingReceived.TryGetValue(player, out var received))
            {
                if (!received)
                {
                    await Task.Delay(1);
                    continue;
                }
            }

            this.pingReceived[player] = false;
            var packet = new EmptyPayload() { Type = PacketType.Ping };
            await SendPacketAsync(socket, packet);
            this.pingSend[player] = DateTime.UtcNow;

            await Task.Delay(1000);
        }
    }

    private static async Task SendPacketAsync(WebSocket socket, IPacketPayload packet)
    {
        var buffer = PacketSerializer.ToByteArray(packet);

        Monitor.Enter(socket);
        try
        {
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE SENDING PACKET (SendPacketAsync): " + e.Message);
        }
        finally
        {
            Monitor.Exit(socket);
        }
    }

    private async Task GameStateBroadcastLoop()
    {
        while (true)
        {
            var startTime = DateTime.UtcNow;

            var packet = new GameStatePayload
            {
                Players = [.. this.players.Values],
                GridState = this.Grid.ToPayload(),
            };

            this.Grid.UpdateBullets(1f);
            this.RegeneratePlayersBullets();
            this.Grid.RegenerateTanks();

            var broadcastTask = this.Broadcast(packet);
            this.playersWhoSentMovementPacketThisTick.Clear();

            var endTime = DateTime.UtcNow;
            var sleepTime = this.broadcastInterval - (endTime - startTime).Milliseconds;

            var tcs = new TaskCompletionSource<bool>();

            this.PlayerSentMovementPacket += (player) =>
            {
                if (this.eagerBroadcast)
                {
                    var alivePlayers = this.players.Values.Where(p => !p.Tank.IsDead);
                    if (this.playersWhoSentMovementPacketThisTick.Count >= alivePlayers.Count())
                    {
                        _ = tcs.TrySetResult(true);
                    }
                }
            };

            if (sleepTime > 0)
            {
                var delayTask = Task.Delay(sleepTime);
                var completedTask = await Task.WhenAny(delayTask, tcs.Task);
                if (completedTask == tcs.Task)
                {
                    Console.WriteLine("All alive players returned their move, broadcasting early.");
                }
            }
            else
            {
                var diff = endTime - startTime;
                Console.Write($"WARNING: GAME STATE BROADCAST TOOK {diff.TotalMilliseconds}ms ");
                Console.WriteLine($"AND EXCEEDED INTERVAL {this.broadcastInterval}ms!");
            }
        }
    }

    private async Task HandleBuffer(WebSocket socket, byte[] buffer)
    {
        Packet packet;
        try
        {
            packet = PacketSerializer.Deserialize(buffer);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE DESERIALIZING PACKET (HandleBuffer): " + e.Message);
            return;
        }

        Player player = this.players[socket];
        if (MovementRestrictedPacketTypes.Contains(packet.Type))
        {
            if (this.players[socket].Tank.IsDead
                || this.playersWhoSentMovementPacketThisTick.Contains(player))
            {
                return;
            }
            else
            {
                this.playersWhoSentMovementPacketThisTick.Add(player);
                this.PlayerSentMovementPacket?.Invoke(player);
            }
        }

        try
        {
            switch (packet.Type)
            {
                case PacketType.TankMovement:
                    var movement = packet.GetPayload<TankMovementPayload>();
                    this.Grid.TryMoveTank(this.players[socket].Tank, movement.Direction);
                    break;

                case PacketType.TankRotation:
                    var rotation = packet.GetPayload<TankRotationPayload>();
                    if (rotation.TankRotation is { } tankRotation)
                    {
                        this.players[socket].Tank.Rotate(tankRotation);
                    }

                    if (rotation.TurretRotation is { } turretRotation)
                    {
                        this.players[socket].Tank.Turret.Rotate(turretRotation);
                    }

                    break;

                case PacketType.TankShoot:
                    _ = this.players[socket].Tank.Turret.TryShoot();
                    break;

                case PacketType.GameData:
                    await this.SendGameData(socket);
                    break;
#if DEBUG
                case PacketType.ShootAll:
                    this.players.Values.ToList().ForEach(x => x.Tank.Turret.TryShoot());
                    break;
#endif
                case PacketType.Pong:
                    this.pingReceived[this.players[socket]] = true;
                    this.players[socket].Ping = (int)(DateTime.UtcNow - this.pingSend[this.players[socket]])!.TotalMilliseconds;
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE HANDLING PACKET (HandleBuffer): " + e.Message);
        }
    }

    private void RegeneratePlayersBullets()
    {
        foreach (Player player in this.players.Values)
        {
            player.Tank.Turret.RegenerateBullets();
        }
    }

    private async Task Broadcast(IPacketPayload packet)
    {
        //var msg = PacketSerializer.Serialize(packet);
        //Console.WriteLine($"Broadcasting: {msg}\n");

        byte[] buffer;
        try
        {
            buffer = PacketSerializer.ToByteArray(packet);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE SERIALIZING PACKET: " + e.Message);
            return;
        }

        foreach (var client in this.players.Keys)
        {
            if (client.State == WebSocketState.Open)
            {
                Monitor.Enter(client);
                try
                {
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR WHILE BROADCASTING PACKET: " + e.Message);
                }
                finally
                {
                    Monitor.Exit(client);
                }
            }
        }
    }
}
