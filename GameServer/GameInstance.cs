using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection;
using GameLogic;
using GameLogic.Networking;
using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents a game instance.
/// </summary>
internal class GameInstance
{
    private readonly ConcurrentDictionary<WebSocket, Connection> connections = new();
    private readonly ConcurrentBag<PlayerConnection> disconnectedConnectionsWhileInGame = [];
    private readonly Logger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    /// <param name="log">The logger.</param>
    public GameInstance(CommandLineOptions options, Logger log)
    {
        this.Options = options;

        this.log = log;
        int seed = options.Seed!.Value;
        int dimension = options.GridDimension;

#if HACKATHON
        bool eagerBroadcast = options.EagerBroadcast;
        string? matchName = options.MatchName;
#else
        bool eagerBroadcast = false;
        string? matchName = null;
#endif

        var version = Assembly.GetExecutingAssembly().GetName().Version!;
        var versionText = $"v{version.Major}.{version.Minor}.{version.Build}";

        this.Settings = new ServerSettings(
            dimension,
            options.NumberOfPlayers,
            seed,
            options.SandboxMode ? null : options.Ticks,
            options.BroadcastInterval,
            options.SandboxMode,
            eagerBroadcast,
            matchName,
            versionText);

        this.Grid = new Grid(dimension, seed);

        this.LobbyManager = new LobbyManager(this, log);
        this.GameManager = new GameManager(this, log);
        this.PlayerManager = new PlayerManager(this, log);
        this.PacketHandler = new PacketHandler(this, log);
        this.PayloadHelper = new PayloadHelper(this);

        PacketSerializer.ExceptionThrew += (Exception ex) =>
        {
            log.Error(ex, "An error has been thrown in PacketSerializer.");
        };

        _ = Task.Run(this.HandleStartGame);
        _ = Task.Run(this.RemoveAbortedConnections);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    /// <param name="log">The logger.</param>
    /// <param name="replayPath">The path to save the replay.</param>
    public GameInstance(CommandLineOptions options, Logger log, string replayPath)
        : this(options, log)
    {
        Debug.Assert(options.SaveReplay, "Replay path provided without saving replay.");
        this.ReplayManager = new ReplayManager(this, replayPath, log);
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

#if STEREO

    /// <summary>
    /// Gets the teams.
    /// </summary>
    public IEnumerable<Team> Teams => this.Players.Select(x => x.Team).Distinct();

#endif

    /// <summary>
    /// Gets the spectator connections.
    /// </summary>
    public IEnumerable<SpectatorConnection> Spectators => this.connections.Values.OfType<SpectatorConnection>();

    /// <summary>
    /// Gets the disconnected in game players.
    /// </summary>
    public IEnumerable<PlayerConnection> DisconnectedInGamePlayers => this.disconnectedConnectionsWhileInGame;

    /// <summary>
    /// Gets the command line options.
    /// </summary>
    public CommandLineOptions Options { get; }

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
            this.log.Information($"Connection added: {connection}");
        }
        else
        {
            this.log.Error($"Failed to add connection: {connection}");
        }
    }

    /// <summary>
    /// Removes a connection.
    /// </summary>
    /// <param name="socket">The socket of the connection to remove.</param>
    public void RemoveConnection(WebSocket socket)
    {
        bool removed;
        Connection? connection;

        lock (this.GameManager)
        {
            removed = this.connections.TryRemove(socket, out connection);
            if (removed)
            {
                this.log.Information($"Connection removed: {connection}");

                if (connection is PlayerConnection p)
                {
                    this.PlayerManager.RemovePlayer(p);
                }
            }
        }

        if (!removed)
        {
            this.log.Error($"Failed to remove connection: {connection}.");
        }
        else if (connection is PlayerConnection player
            && this.GameManager.Status is GameStatus.Running
            && !this.Settings.SandboxMode)
        {
            this.disconnectedConnectionsWhileInGame.Add(player);
            this.log.Information(
                "Connection marked as a disconnected while in game: {player}.", player);
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
            this.log.Verbose("Checking for aborted connections.");

            var abortedConnections = this.connections.Values
            .Where(x => x.Socket.State is WebSocketState.Aborted)
            .ToList();

            this.log.Verbose($"Found {abortedConnections.Count} aborted connections.");

            foreach (Connection connection in abortedConnections)
            {
                lock (this.GameManager)
                {
                    this.log.Information($"Removing aborted connection: {connection}");
                    this.RemoveConnection(connection.Socket);
                }
            }

            await Task.Delay(1000);
        }
    }
}
