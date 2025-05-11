using GameLogic.Networking;
using Serilog;

namespace GameServer;

#if DEBUG

/// <summary>
/// Handles debug packets (only in DEBUG builds).
/// </summary>
internal sealed class DebugPacketHandler(GameInstance game, ILogger logger)
    : IPacketSubhandler
{
    private readonly PlayerActionHandler actionHandler = new(game, logger);

    /// <inheritdoc/>
    public bool CanHandle(Connection connection, Packet packet)
    {
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> HandleAsync(Connection connection, Packet packet)
    {
        ErrorPayload errorPayload;

        switch (packet.Type)
        {
            case PacketType.ForceEndGame:
                if (game.GameManager.Status is GameStatus.Running)
                {
                    logger.Debug("End game forced by {connection}.", connection);
                    game.GameManager.EndGame();
                }
                else
                {
                    errorPayload = new ErrorPayload(
                        PacketType.InvalidPacketUsageError | PacketType.HasPayload,
                        "Cannot force end the game when it is not running");
                    await new ResponsePacket(errorPayload, logger).SendAsync(connection);
                }

                return true;

#if STEREO

            case PacketType.FullyRegenerateAbility:
                var chargePayload = packet.GetPayload<FullyRegenerateAbilityPayload>();
                logger.Debug(
                    "Charge ability {ability} packet received from {connection}.",
                    chargePayload.AbilityType,
                    connection);

                if (connection is PlayerConnection playerConnection)
                {
                    this.actionHandler.FullyRegenerateAbility(playerConnection, chargePayload.AbilityType);
                }
                else
                {
                    foreach (var player in game.Players)
                    {
                        this.actionHandler.FullyRegenerateAbility(player, chargePayload.AbilityType);
                    }
                }

                return true;

#endif

            case PacketType.GlobalAbilityUse:
                var abilityUsePayload = packet.GetPayload<AbilityUsePayload>();

                logger.Debug(
                    "Global ability use (type) packet received from {connection}.",
                    abilityUsePayload.AbilityType,
                    connection);

                foreach (var player in game.Players)
                {
                    var action = this.actionHandler.GetAbilityAction(abilityUsePayload.AbilityType, player.Instance);
                    if (action is null)
                    {
                        logger.Warning("Ability type '{type}' is not valid.", abilityUsePayload.AbilityType);
                        return false;
                    }

                    action();
                }

                return true;

#if !STEREO

            case PacketType.GiveSecondaryItem:
                var giveSecondaryItemPayload = packet.GetPayload<GiveSecondaryItemPayload>();

                logger.Debug(
                    "Give secondary item (type) packet received from {connection}.",
                    giveSecondaryItemPayload.Item,
                    connection);

                if (connection is PlayerConnection playerConnection)
                {
                    playerConnection.Instance.Tank.SecondaryItemType = giveSecondaryItemPayload.Item;
                }
                else
                {
                    errorPayload = new ErrorPayload(
                        PacketType.InvalidPacketUsageErrorWithPayload,
                        "Cannot use GiveSecondaryItem packet when not a player.");
                    await new ResponsePacket(errorPayload, logger).SendAsync(connection);
                }

                return true;

            case PacketType.GlobalGiveSecondaryItem:
                var globalGiveSecondaryItemPayload = packet.GetPayload<GlobalGiveSecondaryItemPayload>();

                logger.Debug(
                    "Global give secondary item (type) packet received from {connection}.",
                    globalGiveSecondaryItemPayload.Item,
                    connection);

                foreach (var player in game.Players)
                {
                    player.Instance.Tank.SecondaryItemType = globalGiveSecondaryItemPayload.Item;
                }

                return true;

#endif

            default:
                return false;
        }
    }
}

#endif
