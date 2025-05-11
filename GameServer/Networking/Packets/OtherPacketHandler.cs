using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Handles general packets not specific to player, spectator, or debug.
/// </summary>
internal sealed class OtherPacketHandler(GameInstance game, ILogger logger)
    : IPacketSubhandler
{
    /// <inheritdoc/>
    public bool CanHandle(Connection connection, Packet packet)
    {
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> HandleAsync(Connection connection, Packet packet)
    {
        switch (packet.Type)
        {
            case PacketType.Pong:
                connection.HasSentPong = true;
                logger.Verbose("Received pong from {connection}.", connection);

                if (connection is PlayerConnection player)
                {
                    player.Instance.Ping = (int)(DateTime.UtcNow - player.LastPingSentTime).TotalMilliseconds;
                }

                if (connection.IsSecondPingAttempt)
                {
                    connection.IsSecondPingAttempt = false;
                    logger.Information("Client responded to second ping. ({connection})", connection);
                }

                return true;

            case PacketType.LobbyDataRequest:
                await game.LobbyManager.SendLobbyDataTo(connection);
                return true;

            case PacketType.GameStatusRequest:
                var statusPayload = game.PayloadHelper.GetGameStatusPayload();
                await new ResponsePacket(statusPayload, logger).SendAsync(connection);
                return true;

            case PacketType.ReadyToReceiveGameState:
                logger.Debug("Client is ready to receive game state ({connection}).", connection);
                connection.IsReadyToReceiveGameState = true;
                return true;

            default:
                return false;
        }
    }
}
