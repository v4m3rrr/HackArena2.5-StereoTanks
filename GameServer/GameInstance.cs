using System.Collections.Concurrent;
using System.Net.WebSockets;
using GameLogic;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Represents a single match instance of the game.
/// </summary>
internal sealed class GameInstance
{
    private readonly ConcurrentDictionary<WebSocket, Connection> connections = new();
    private readonly ConcurrentBag<PlayerConnection> disconnectedConnectionsWhileInGame = [];
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command-line options.</param>
    /// <param name="logger">The logger instance.</param>
    public GameInstance(CommandLineOptions options, ILogger logger)
        : this(options, logger, replayPath: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command-line options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="replayPath">Optional replay file path.</param>
    public GameInstance(CommandLineOptions options, ILogger logger, string? replayPath)
    {
        this.Options = options;
        this.logger = logger;

        int seed = options.Seed!.Value;
        int dimension = options.GridDimension;

        this.Settings = new ServerSettings(
            dimension,
            options.NumberOfPlayers,
            seed,
            options.SandboxMode ? null : options.Ticks,
            options.BroadcastInterval,
            options.SandboxMode,
            GetVersion())
        {
#if HACKATHON
            EagerBroadcast = options.EagerBroadcast,
            MatchName = options.MatchName,
#endif
        };

        this.Grid = new Grid(dimension, seed);

        this.Systems = new GameSystems(this.Grid);
        this.TickLoop = new GameTickLoop(this, logger);
        this.StateUpdater = new GameStateUpdater(this.Systems);

        this.LobbyManager = new LobbyManager(this, logger);
        this.GameManager = new GameManager(this, logger);
        this.PlayerManager = new PlayerManager(this, logger);
        this.PacketHandler = new PacketHandler(this, logger);
        this.PayloadHelper = new PayloadHelper(this);
        this.ReplayManager = replayPath is not null ? new ReplayManager(this, replayPath, logger) : null;

        PacketSerializer.ExceptionThrew += ex => logger.Error(ex, "PacketSerializer threw an error.");

        _ = Task.Run(this.HandleStartGame);
        _ = Task.Run(this.RemoveAbortedConnections);
    }

    /// <summary>
    /// Gets all current connections.
    /// </summary>
    public IEnumerable<Connection> Connections => this.connections.Values;

    /// <summary>
    /// Gets all WebSocket references for active connections.
    /// </summary>
    public IEnumerable<WebSocket> ConnectionSockets => this.connections.Keys;

    /// <summary>
    /// Gets the list of player connections.
    /// </summary>
    public IEnumerable<PlayerConnection> Players => this.connections.Values.OfType<PlayerConnection>();

    /// <summary>
    /// Gets the list of spectator connections.
    /// </summary>
    public IEnumerable<SpectatorConnection> Spectators => this.connections.Values.OfType<SpectatorConnection>();

    /// <summary>
    /// Gets the list of players that disconnected during the game.
    /// </summary>
    public IEnumerable<PlayerConnection> DisconnectedInGamePlayers => this.disconnectedConnectionsWhileInGame;

    /// <summary>
    /// Gets the command-line options.
    /// </summary>
    public CommandLineOptions Options { get; }

    /// <summary>
    /// Gets the server settings for this instance.
    /// </summary>
    public ServerSettings Settings { get; }

    /// <summary>
    /// Gets the grid representing the game map.
    /// </summary>
    public Grid Grid { get; }

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
    /// Gets the packet handler.
    /// </summary>
    public PacketHandler PacketHandler { get; }

    /// <summary>
    /// Gets the payload helper for serializing responses.
    /// </summary>
    public PayloadHelper PayloadHelper { get; }

    /// <summary>
    /// Gets the replay manager.
    /// </summary>
    public ReplayManager? ReplayManager { get; }

#if STEREO
    /// <summary>
    /// Gets the list of teams (only for Stereo mode).
    /// </summary>
    public IEnumerable<Team> Teams => this.Players.Select(x => x.Team).Distinct();
#endif

    /// <summary>
    /// Gets the tick loop for the game instance.
    /// </summary>
    public GameTickLoop TickLoop { get; }

    /// <summary>
    /// Gets the state updater for the game instance.
    /// </summary>
    public GameStateUpdater StateUpdater { get; }

    /// <summary>
    /// Gets the game systems for the game instance.
    /// </summary>
    public GameSystems Systems { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the game instance is valid.
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Adds a connection to the game instance.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    public void AddConnection(Connection connection)
    {
        if (this.connections.TryAdd(connection.Socket, connection))
        {
            this.logger.Information("Connection added: {connection}", connection);
        }
        else
        {
            this.logger.Error("Failed to add connection: {connection}", connection);
        }
    }

    /// <summary>
    /// Removes a connection from the game instance.
    /// </summary>
    /// <param name="socket">The WebSocket of the connection to remove.</param>
    public void RemoveConnection(WebSocket socket)
    {
        if (this.connections.TryRemove(socket, out var connection))
        {
            this.logger.Information("Connection removed: {connection}", connection);

            if (connection is PlayerConnection player)
            {
                this.PlayerManager.RemovePlayer(player);

                if (this.GameManager.Status is GameStatus.Running && !this.Settings.SandboxMode)
                {
                    this.disconnectedConnectionsWhileInGame.Add(player);
                    this.logger.Information("Player marked as disconnected in game: {player}", player);
                }
            }
        }
        else
        {
            this.logger.Error("Failed to remove connection: {socket}", socket);
        }
    }

    private static string GetVersion()
    {
        var version = typeof(GameInstance).Assembly.GetName().Version!;
        return $"v{version.Major}.{version.Minor}.{version.Build}";
    }

    private async Task HandleStartGame()
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
            var aborted = this.connections.Values.Where(c => c.Socket.State is WebSocketState.Aborted).ToList();
            foreach (var conn in aborted)
            {
                this.logger.Information("Removing aborted connection: {connection}", conn);
                this.RemoveConnection(conn.Socket);
            }

            await Task.Delay(1000);
        }
    }
}
