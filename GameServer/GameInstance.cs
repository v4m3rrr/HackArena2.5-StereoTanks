using System.Collections.Concurrent;
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

    private readonly Dictionary<WebSocket, Player> players = [];
    private readonly List<Player> playersWhoSentMovementPacketThisTick = [];

    private readonly ConcurrentDictionary<Player, DateTime> pingSend = [];
    private readonly ConcurrentDictionary<Player, bool> pingReceived = [];

    private readonly List<WebSocket> spectators = [];

    private readonly int broadcastInterval;
    private readonly bool eagerBroadcast;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    public GameInstance(CommandLineOptions options)
    {
        int seed = options.Seed!.Value;
        int dimension = options.GridDimension;
        this.Grid = new Grid(dimension, seed);

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
    /// Adds a player to the game.
    /// </summary>
    /// <param name="socket">The socket of the player to add.</param>
    public void AddPlayer(WebSocket socket)
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
    }

    /// <summary>
    /// Removes a player from the game.
    /// </summary>
    /// <param name="socket">The socket of the player to remove.</param>
    public void RemovePlayer(WebSocket socket)
    {
        var player = this.players[socket];
        _ = this.players.Remove(socket);
        _ = this.Grid.RemoveTank(player);
    }

    /// <summary>
    /// Adds a spectator to the game.
    /// </summary>
    /// <param name="socket">The socket of the spectator to add.</param>
    public void AddSpectator(WebSocket socket)
    {
        this.spectators.Add(socket);
    }

    /// <summary>
    /// Removes a spectator from the game.
    /// </summary>
    /// <param name="socket">The socket of the spectator to remove.</param>
    public void RemoveSpectator(WebSocket socket)
    {
        _ = this.spectators.Remove(socket);
    }

    /// <summary>
    /// Handles a connection.
    /// </summary>
    /// <param name="socket">The socket of the client to handle.</param>
    /// <param name="ip">The IP address of the client.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleConnection(WebSocket socket, string ip)
    {
        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            var buffer = new byte[1024 * 32];
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30000));
            try
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                break;
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
                await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Internal server error", CancellationToken.None);
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                await this.HandleBuffer(socket, buffer);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine($"Received close message from client ({ip})");

                if (this.IsSpecator(socket))
                {
                    this.RemoveSpectator(socket);
                }
                else
                {
                    this.RemovePlayer(socket);
                }
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
        _ = this.players.TryGetValue(socket, out var player);
        var response = new GameDataPayload(
            player?.Id,
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

    private bool IsSpecator(WebSocket socket)
    {
        return this.spectators.Contains(socket);
    }

    private async Task GameStateBroadcastLoop()
    {
        while (true)
        {
            var startTime = DateTime.UtcNow;

            this.Grid.UpdateBullets(1f);
            this.RegeneratePlayersBullets();
            this.Grid.RegenerateTanks();

            var broadcastTask = this.BroadcastGameState();
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

    private async Task<bool?> HandlePlayerPacket(WebSocket socket, Packet packet)
    {
        Player player = this.players[socket];
        if (MovementRestrictedPacketTypes.Contains(packet.Type))
        {
            if (player.Tank.IsDead
                || this.playersWhoSentMovementPacketThisTick.Contains(player))
            {
                return true;
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
                    this.Grid.TryMoveTank(player.Tank, movement.Direction);
                    break;

                case PacketType.TankRotation:
                    var rotation = packet.GetPayload<TankRotationPayload>();
                    if (rotation.TankRotation is { } tankRotation)
                    {
                        player.Tank.Rotate(tankRotation);
                    }

                    if (rotation.TurretRotation is { } turretRotation)
                    {
                        player.Tank.Turret.Rotate(turretRotation);
                    }

                    break;

                case PacketType.TankShoot:
                    _ = player.Tank.Turret.TryShoot();
                    break;

                case PacketType.GameData:
                    await this.SendGameData(socket);
                    break;

                case PacketType.Pong:
                    this.pingReceived[player] = true;
                    player.Ping = (int)(DateTime.UtcNow - this.pingSend[player])!.TotalMilliseconds;
                    break;

                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE HANDLING PACKET (HandlePlayerPacket): " + e.Message);
            return null;
        }

        return true;
    }

    private async Task<bool?> HandleSpectatorPacket(WebSocket socket, Packet packet)
    {
        try
        {
            switch (packet.Type)
            {
                case PacketType.Ping:
                    var pongPacket = new EmptyPayload() { Type = PacketType.Pong };
                    await SendPacketAsync(socket, pongPacket);
                    break;

                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE HANDLING PACKET (HandlePlayerPacket): " + e.Message);
            return null;
        }

        return true;
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

        bool? packetHandled = null;
        try
        {
            bool isSpectator = this.IsSpecator(socket);

            packetHandled = isSpectator
                ? await this.HandleSpectatorPacket(socket, packet)
                : await this.HandlePlayerPacket(socket, packet);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE HANDLING PACKET 2 (HandleBuffer): " + e.Message);
        }

        if (packetHandled is true)
        {
            return;
        }

        try
        {
            switch (packet.Type)
            {
#if DEBUG
                case PacketType.ShootAll:
                    this.players.Values.ToList().ForEach(x => x.Tank.Turret.TryShoot());
                    break;
#endif
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

    private async Task BroadcastGameState()
    {
        byte[] buffer;
        foreach (var client in this.players.Keys.Concat(this.spectators).ToList())
        {
            GameStatePayload packet;

            if (this.IsSpecator(client))
            {
                packet = new GameStatePayload([.. this.players.Values], this.Grid.ToStatePayload());
            }
            else
            {
                var player = this.players[client];
                packet = new GameStatePayload.ForPlayer(player, [.. this.players.Values], this.Grid.ToStatePayload());
            }

            SerializationContext context = this.IsSpecator(client)
                ? new SerializationContext.Spectator()
                : new SerializationContext.Player(this.players[client].Id);

            var converters = GameStatePayload.GetConverters(context);
            //Console.WriteLine(PacketSerializer.Serialize(packet, converters, indented: true));

            try
            {
                buffer = PacketSerializer.ToByteArray(packet, converters);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WHILE SERIALIZING PACKET: " + e.Message);
                continue;
            }

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
