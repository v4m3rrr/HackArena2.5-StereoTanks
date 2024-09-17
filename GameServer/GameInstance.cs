using System.Collections.Concurrent;
using System.Net.WebSockets;
using GameLogic;
using GameLogic.Networking;
using Newtonsoft.Json;

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

    private static readonly uint[] Colors =
    [
        /* ABGR */
        0xFFFFA600,
        0xFFFF5AF9,
        0xFF1A9BF9,
        0xFF3FD47A,
    ];

    private readonly List<Player> playersWhoSentMovementPacketThisTick = [];

    private readonly ConcurrentDictionary<Player, DateTime> pingSend = [];
    private readonly ConcurrentDictionary<Player, bool> pingReceived = [];

    private readonly List<WebSocket> spectators = [];

    private readonly ServerSettings settings;

    private DateTime? startTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    public GameInstance(CommandLineOptions options)
    {
        int seed = options.Seed!.Value;
        int dimension = options.GridDimension;
        this.Grid = new Grid(dimension, seed);

        this.settings = new ServerSettings(
            dimension,
            options.NumberOfPlayers,
            seed,
            options.BroadcastInterval,
            options.EagerBroadcast);

        PacketSerializer.ExceptionThrew += (Exception ex) =>
        {
            Console.WriteLine(
                $"[ERROR] An error has been thrown in the PacketSerializer: {ex.Message}");
        };

        _ = Task.Run(HandleStartGame);
    }

    private event Action<Player>? PlayerSentMovementPacket;

    /// <summary>
    /// Gets the players of the game.
    /// </summary>
    public Dictionary<WebSocket, Player> Players { get; } = [];

    /// <summary>
    /// Gets the grid of the game.
    /// </summary>
    public Grid Grid { get; private set; }

    /// <summary>
    /// Adds a player to the game.
    /// </summary>
    /// <param name="socket">The socket of the player to add.</param>
#if DEBUG
    public void AddPlayer(WebSocket socket, string nickname, bool quickJoin)
#else
    public void AddPlayer(WebSocket socket, string nickname)
#endif
    {
        string id;
        do
        {
            id = Guid.NewGuid().ToString();
        } while (this.Players.Values.Any(p => p.Id == id));

        var color = this.GetPlayerColor();

        Player player;
#if DEBUG
        if (quickJoin)
        {
            player = new Player(id, $"Player {this.Players.Count + 1}", color);
        }
        else
        {
#endif
        player = new Player(id, nickname, color);
#if DEBUG
        }
#endif

        _ = this.Grid.GenerateTank(player);
        this.Players.Add(socket, player);

        foreach (var ws in this.Players.Keys.Concat(this.spectators).ToList())
        {
            _ = Task.Run(() => this.SendLobbyData(ws));
        }

#if DEBUG
        if (quickJoin && this.startTime is null)
        {
            this.StartGame();
        }
#endif
    }

    /// <summary>
    /// Removes a player from the game.
    /// </summary>
    /// <param name="socket">The socket of the player to remove.</param>
    public void RemovePlayer(WebSocket socket)
    {
        var player = this.Players[socket];
        _ = this.Players.Remove(socket);
        _ = this.Grid.RemoveTank(player);
    }

    /// <summary>
    /// Adds a spectator to the game.
    /// </summary>
    /// <param name="socket">The socket of the spectator to add.</param>
    public void AddSpectator(WebSocket socket)
    {
        this.spectators.Add(socket);
        _ = Task.Run(() => this.SendLobbyData(socket));
    }

    /// <summary>
    /// Removes a spectator from the game.
    /// </summary>
    /// <param name="socket">The socket of the spectator to remove.</param>
    public void RemoveSpectator(WebSocket socket)
    {
        _ = this.spectators.Remove(socket);
    }

    public void StartGame()
    {
        this.startTime ??= DateTime.UtcNow;
        _ = Task.Run(this.GameStateBroadcastLoop);
    }

    public async Task HandleStartGame()
    {
        while (this.startTime is null && this.Players.Count < this.settings.NumberOfPlayers)
        {
            await Task.Delay(1000);
        }

        await Task.Delay(2000);

        foreach (var player in this.Players)
        {
            var packet = new EmptyPayload() { Type = PacketType.GameStart };
            await SendPacketAsync(player.Key, packet);
        }

        await Task.Delay(2000);

        if (this.startTime is null)
        {
            this.StartGame();
        }
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
    public async Task SendLobbyData(WebSocket socket)
    {
        _ = this.Players.TryGetValue(socket, out var player);

        var response = new LobbyDataPayload(
            player?.Id,
            [.. this.Players.Values],
            this.settings);

        var converters = LobbyDataPayload.GetConverters();
        await SendPacketAsync(socket, response, converters);
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
            var player = this.Players[socket];
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

    private static async Task SendPacketAsync(
        WebSocket socket,
        IPacketPayload packet,
        List<JsonConverter>? converters = null)
    {
        var buffer = PacketSerializer.ToByteArray(packet, converters ?? []);

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

    private uint GetPlayerColor()
    {
        foreach (uint color in Colors)
        {
            if (!this.Players.Values.Any(p => p.Color == color))
            {
                return color;
            }
        }

        return (uint)((0xFF << 24) | Random.Next(0xFFFFFF));
    }

    private async Task GameStateBroadcastLoop()
    {
        while (true)
        {
            var startTime = DateTime.UtcNow;

            this.Grid.UpdateBullets(1f);
            this.RegeneratePlayersBullets();
            this.Grid.UpdateTanksRegenerationProgress();
            this.Grid.UpdatePlayersVisibilityGrids();
            this.Grid.UpdateZones();

            await this.BroadcastGameState();
            this.playersWhoSentMovementPacketThisTick.Clear();

            var endTime = DateTime.UtcNow;
            var sleepTime = this.settings.BroadcastInterval - (endTime - startTime).Milliseconds;

            var tcs = new TaskCompletionSource<bool>();

            this.PlayerSentMovementPacket += (player) =>
            {
                if (this.settings.EagerBroadcast)
                {
                    var alivePlayers = this.Players.Values.Where(p => !p.Tank.IsDead);
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
                Console.WriteLine($"AND EXCEEDED INTERVAL {this.settings.BroadcastInterval}ms!");
            }
        }
    }

    private async Task<bool?> HandlePlayerPacket(WebSocket socket, Packet packet)
    {
        Player player = this.Players[socket];
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

                case PacketType.LobbyData:
                    await this.SendLobbyData(socket);
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
                    this.Players.Values.ToList().ForEach(x => x.Tank.Turret.TryShoot());
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
        foreach (Player player in this.Players.Values)
        {
            player.Tank.Turret.RegenerateBullets();
        }
    }

    private async Task BroadcastGameState()
    {
        float time = (float)(DateTime.UtcNow - (this.startTime ?? DateTime.UtcNow)).TotalMilliseconds;

        byte[] buffer;
        foreach (var client in this.Players.Keys.Concat(this.spectators).ToList())
        {
            GameStatePayload packet;

            if (this.IsSpecator(client))
            {
                packet = new GameStatePayload(time, [.. this.Players.Values], this.Grid);
            }
            else
            {
                var player = this.Players[client];
                packet = new GameStatePayload.ForPlayer(time, player, [.. this.Players.Values], this.Grid);
            }

            GameSerializationContext context = this.IsSpecator(client)
                ? new GameSerializationContext.Spectator()
                : new GameSerializationContext.Player(this.Players[client]);

            var converters = GameStatePayload.GetConverters(context);

            try
            {
                buffer = PacketSerializer.ToByteArray(packet, converters);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WHILE SERIALIZING PACKET: " + e.Message);
                continue;
            }

            //var message = PacketSerializer.Serialize(packet, converters, indented: true);
            //Console.WriteLine(message);

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
