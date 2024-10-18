using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using GameLogic;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents a game instance.
/// </summary>
internal class GameInstance
{
    private readonly ConcurrentDictionary<WebSocket, Connection> connections = new();
    private readonly ConcurrentBag<PlayerConnection> disconnectedConnectionsWhileInGame = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    public GameInstance(CommandLineOptions options)
    {
        int seed = options.Seed!.Value;
        int dimension = options.GridDimension;

#if HACKATHON
        bool eagerBroadcast = options.EagerBroadcast;
        string? matchName = options.MatchName;
#else
        bool eagerBroadcast = false;
        string? matchName = null;
#endif

        this.Settings = new ServerSettings(
            dimension,
            options.NumberOfPlayers,
            seed,
            options.Ticks,
            options.BroadcastInterval,
            eagerBroadcast,
            matchName);

        this.Grid = new Grid(dimension, seed);

        this.LobbyManager = new LobbyManager(this);
        this.GameManager = new GameManager(this);
        this.PlayerManager = new PlayerManager(this);
        this.PacketHandler = new PacketHandler(this);
        this.PayloadHelper = new PayloadHelper(this);

        PacketSerializer.ExceptionThrew += (Exception ex) =>
        {
            Console.WriteLine("[ERROR] An error has been thrown in the PacketSerializer:");
            Console.WriteLine("[^^^^^] {0}", ex.Message);
        };

        _ = Task.Run(this.HandleStartGame);
        _ = Task.Run(this.RemoveAbortedConnections);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    /// <param name="replayPath">The path to save the replay.</param>
    public GameInstance(CommandLineOptions options, string replayPath)
        : this(options)
    {
        Debug.Assert(options.SaveReplay, "Replay path provided without saving replay.");
        this.ReplayManager = new ReplayManager(this, replayPath);
    }

    /// <summary>
    /// Gets the connections.
    /// </summary>
    public IEnumerable<Connection> Connections => this.connections.Values;

    /// <summary>
    /// Gets the connection sockets.
    /// </summary>
    public IEnumerable<WebSocket> ConnectionSockets => this.connections.Keys;

    /// <summary>
    /// Gets the player connections.
    /// </summary>
    public IEnumerable<PlayerConnection> Players => this.connections.Values.OfType<PlayerConnection>();

    /// <summary>
    /// Gets the spectator connections.
    /// </summary>
    public IEnumerable<SpectatorConnection> Spectators => this.connections.Values.OfType<SpectatorConnection>();

    /// <summary>
    /// Gets the disconnected in game players.
    /// </summary>
    public IEnumerable<PlayerConnection> DisconnectedInGamePlayers => this.disconnectedConnectionsWhileInGame;

    /// <summary>
    /// Gets the lobby manager.
    /// </summary>
    public LobbyManager LobbyManager { get; }

    /// <summary>
    /// Gets the game manager.
    /// </summary>
    public GameManager GameManager { get; }

    /// <summary>
    /// Gets the player manager.
    /// </summary>
    public PlayerManager PlayerManager { get; }

    /// <summary>
    /// Gets the replay manager.
    /// </summary>
    public ReplayManager? ReplayManager { get; }

    /// <summary>
    /// Gets the packet handler.
    /// </summary>
    public PacketHandler PacketHandler { get; }

    /// <summary>
    /// Gets the server settings.
    /// </summary>
    public ServerSettings Settings { get; }

    /// <summary>
    /// Gets the payload helper.
    /// </summary>
    public PayloadHelper PayloadHelper { get; }

    /// <summary>
    /// Gets the grid of the game.
    /// </summary>
    public Grid Grid { get; private set; }

    /// <summary>
    /// Adds a connection.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    public void AddConnection(Connection connection)
    {
        if (this.connections.TryAdd(connection.Socket, connection))
        {
            Console.WriteLine($"[INFO] Connection added: {connection}");
        }
        else
        {
            Console.WriteLine($"[ERROR] Failed to add connection: {connection}");
        }
    }

    /// <summary>
    /// Removes a connection.
    /// </summary>
    /// <param name="socket">The socket of the connection to remove.</param>
    public void RemoveConnection(WebSocket socket)
    {
        if (this.connections.TryRemove(socket, out var connection))
        {
            Console.WriteLine($"[INFO] Connection removed: {connection}.");

            if (connection is PlayerConnection p && this.GameManager.Status is GameStatus.Running)
            {
                this.disconnectedConnectionsWhileInGame.Add(p);
                Console.WriteLine(
                    $"[INFO] Connection added as a disconnected while in game: {p}.");
            }
        }
        else
        {
            Console.WriteLine($"[ERROR] Failed to remove connection: {connection}.");
        }

        if (connection is PlayerConnection player)
        {
            this.PlayerManager.RemovePlayer(player);
        }
    }

    /// <summary>
    /// Handles the start of the game.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleStartGame()
    {
        while (this.GameManager.Status is GameStatus.InLobby && this.Players.Count() < this.Settings.NumberOfPlayers)
        {
            await Task.Delay(1000);
        }

        this.GameManager.StartGame();
    }

    private async Task RemoveAbortedConnections()
    {
        while (true)
        {
            var abortedConnections = this.connections.Values
            .Where(x => x.Socket.State is WebSocketState.Aborted)
            .ToList();

            foreach (Connection connection in abortedConnections)
            {
                Console.WriteLine($"[INFO] Removing aborted connection: {connection}");
                this.RemoveConnection(connection.Socket);
            }

            await Task.Delay(1000);
        }
    }
}
