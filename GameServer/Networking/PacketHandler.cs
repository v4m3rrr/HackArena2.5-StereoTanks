using System.Collections.Concurrent;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Handles incoming packets from players and spectators.
/// </summary>
internal sealed class PacketHandler
{
    private readonly ILogger logger;
    private readonly List<IPacketSubhandler> subhandlers;
    private readonly PlayerPacketHandler playerHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketHandler"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <param name="logger">The logger.</param>
    public PacketHandler(GameInstance game, ILogger logger)
    {
        this.logger = logger;
        this.playerHandler = new PlayerPacketHandler(
            game,
#if HACKATHON
            p => this.HackathonBotMadeAction?.Invoke(this, p),
#endif
            logger);

        this.subhandlers = [
            this.playerHandler,
            new SpectatorPacketHandler(game, logger),
#if DEBUG
            new DebugPacketHandler(game, logger),
#endif
            new OtherPacketHandler(game, logger),
        ];
    }

#if HACKATHON

    /// <summary>
    /// Occurs when a hackathon bot makes an action.
    /// </summary>
    public event EventHandler<PlayerConnection>? HackathonBotMadeAction;

    /// <summary>
    /// Gets the list of actions for hackathon bots.
    /// </summary>
    public ConcurrentDictionary<PlayerConnection, Action> HackathonBotActions
        => this.playerHandler.HackathonBotActions;
#endif

    /// <summary>
    /// Handles the incoming buffer from a connection.
    /// </summary>
    /// <param name="connection">The connection from which the buffer was received.</param>
    /// <param name="buffer">The buffer containing the serialized packet data.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task HandleBufferAsync(Connection connection, byte[] buffer)
    {
        Packet packet;
        try
        {
            packet = PacketSerializer.Deserialize(buffer);
        }
        catch (Exception ex)
        {
            this.logger.Error(ex, "Deserialization failed. ({connection})", connection);
            return;
        }

        bool handled = false;
        foreach (var handler in this.subhandlers)
        {
            if (handler.CanHandle(connection, packet))
            {
                handled |= await handler.HandleAsync(connection, packet);
            }
        }

        if (!handled)
        {
            var payload = new ErrorPayload(PacketType.InvalidPacketTypeErrorWithPayload, $"Unhandled packet: {packet.Type}");
            await new ResponsePacket(payload, this.logger).SendAsync(connection);
        }
    }
}
