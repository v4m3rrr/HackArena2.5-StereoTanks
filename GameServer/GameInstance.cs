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
    /// <summary>
    /// Initializes a new instance of the <see cref="GameInstance"/> class.
    /// </summary>
    /// <param name="options">The command line options.</param>
    public GameInstance(CommandLineOptions options)
    {
        int seed = options.Seed!.Value;
        int dimension = options.GridDimension;
        this.Grid = new Grid(dimension, seed);

        this.LobbyManager = new LobbyManager(this);
        this.GameManager = new GameManager(this);
        this.PlayerManager = new PlayerManager(this);
        this.SpectatorManager = new SpectatorManager();
        this.PacketHandler = new PacketHandler(this);

        this.Settings = new ServerSettings(
            dimension,
            options.NumberOfPlayers,
            seed,
            options.Ticks,
            options.BroadcastInterval,
            options.EagerBroadcast);

        PacketSerializer.ExceptionThrew += (Exception ex) =>
        {
            Console.WriteLine(
                $"[ERROR] An error has been thrown in the PacketSerializer: {ex.Message}");
        };

        _ = Task.Run(this.HandleStartGame);
    }

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
    /// Gets the spectator manager.
    /// </summary>
    public SpectatorManager SpectatorManager { get; }

    /// <summary>
    /// Gets the packet handler.
    /// </summary>
    public PacketHandler PacketHandler { get; }

    /// <summary>
    /// Gets the server settings.
    /// </summary>
    public ServerSettings Settings { get; }

    /// <summary>
    /// Gets the grid of the game.
    /// </summary>
    public Grid Grid { get; private set; }

    /// <summary>
    /// Handles the start of the game.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleStartGame()
    {
        while (this.GameManager.Status is GameStatus.InLobby && this.PlayerManager.Players.Count < this.Settings.NumberOfPlayers)
        {
            await Task.Delay(1000);
        }

        this.GameManager.StartGame();
    }

    /// <summary>
    /// Handles a connection.
    /// </summary>
    /// <param name="socket">The socket of the client to handle.</param>
    public void HandleConnection(WebSocket socket)
    {
        _ = Task.Run(() => this.PacketHandler.HandleConnection(socket));
    }

    /// <summary>
    /// Sends a packet to a player or a spectator.
    /// </summary>
    /// <param name="socket">The socket of the player or spectator to send the packet to.</param>
    /// <param name="packet">The packet to send.</param>
    /// <param name="converters">The converters to use when serializing the packet.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendPacketAsync(
        WebSocket socket,
        IPacketPayload packet,
        List<JsonConverter>? converters = null)
    {
        if (this.PlayerManager.Players.ContainsKey(socket))
        {
            await this.SendPlayerPacketAsync(socket, packet, converters);
        }
        else if (this.SpectatorManager.Spectators.ContainsKey(socket))
        {
            await this.SendSpectatorPacketAsync(socket, packet, converters);
        }
        else
        {
            Console.WriteLine(
                "ERROR WHILE SENDING PACKET (SendPacketAsync): The socket is not a player or a spectator.");
        }
    }

    /// <summary>
    /// Sends a packet to a player.
    /// </summary>
    /// <param name="socket">The socket of the player to send the packet to.</param>
    /// <param name="packet">The packet to send.</param>
    /// <param name="converters">The converters to use when serializing the packet.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendPlayerPacketAsync(
        WebSocket socket,
        IPacketPayload packet,
        List<JsonConverter>? converters = null)
    {
        var options = new SerializationOptions() { TypeOfPacketType = this.PlayerManager.Players[socket].ConnectionData.TypeOfPacketType };
        var buffer = PacketSerializer.ToByteArray(packet, converters ?? [], options);

        Monitor.Enter(socket);

        try
        {
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE SENDING PACKET (SendPlayerPacketAsync): " + e.Message);
        }
        finally
        {
            Monitor.Exit(socket);
        }
    }

    /// <summary>
    /// Sends a packet to a spectator.
    /// </summary>
    /// <param name="socket">The socket of the spectator to send the packet to.</param>
    /// <param name="packet">The packet to send.</param>
    /// <param name="converters">The converters to use when serializing the packet.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendSpectatorPacketAsync(
        WebSocket socket,
        IPacketPayload packet,
        List<JsonConverter>? converters = null)
    {
        var buffer = PacketSerializer.ToByteArray(packet, converters ?? [], SerializationOptions.Default);

        Monitor.Enter(socket);

        try
        {
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE SENDING PACKET (SendSpectatorPacketAsync): " + e.Message);
        }
        finally
        {
            Monitor.Exit(socket);
        }
    }
}
