using System.Collections.Concurrent;
using System.Text.Json;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Handles packets coming from player connections.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger instance.</param>
internal sealed class PlayerPacketHandler(
    GameInstance game,
#if HACKATHON
    Action<PlayerConnection>? onHackathonBotMadeAction,
#endif
    ILogger logger)
    : IPacketSubhandler
{
    private readonly PlayerActionHandler actionHandler = new(game, logger);

#if STEREO
    private readonly GoToService goToService = new(game, logger);
#endif

#if HACKATHON

    /// <summary>
    /// Gets the list of actions for hackathon bots.
    /// </summary>
    public ConcurrentDictionary<PlayerConnection, Action> HackathonBotActions { get; } = [];

#endif

    /// <inheritdoc/>
    public bool CanHandle(Connection connection, Packet packet)
    {
        return connection is PlayerConnection;
    }

    /// <inheritdoc/>
    public Task<bool> HandleAsync(Connection connection, Packet packet)
    {
        var player = (PlayerConnection)connection;

        if (!packet.Type.HasFlag(PacketType.PlayerResponseActionGroup))
        {
            return Task.FromResult(false);
        }

#if HACKATHON

        if (!this.ValidateGameStateForBot(player, packet))
        {
            return Task.FromResult(true);
        }

#endif

        if (!this.ValidateTickAndAlive(player, packet))
        {
            return Task.FromResult(true);
        }

        this.DispatchPlayerAction(player, packet);
        this.RegisterPlayerAction(player);
        return Task.FromResult(true);
    }

    private bool ValidateTickAndAlive(PlayerConnection player, Packet packet)
    {
        lock (player)
        {
            if (player.HasMadeActionThisTick)
            {
#if HACKATHON && STEREO
                if (player.IsHackathonBot || packet.Type != PacketType.GoTo)
#elif STEREO
                if (packet.Type != PacketType.GoTo)
#endif
                {
                    this.SendWarning(player, PacketType.PlayerAlreadyMadeActionWarning);
                }

                return false;
            }
        }

        if (player.Instance.Tank.IsDead && packet.Type is not PacketType.Pass)
        {
            this.SendWarning(player, PacketType.ActionIgnoredDueToDeadWarning);
            return false;
        }

        return true;
    }

    private void DispatchPlayerAction(PlayerConnection player, Packet packet)
    {
        switch (packet.Type)
        {
            case PacketType.Movement:
                var move = packet.GetPayload<MovementPayload>();
                this.actionHandler.HandleMovement(player, move.Direction);
                break;

            case PacketType.Rotation:
                var rot = packet.GetPayload<RotationPayload>();
                this.actionHandler.HandleRotation(player, rot.TankRotation, rot.TurretRotation);
                break;

#if STEREO

            case PacketType.GoTo:
                if (this.goToService.TryResolve(player, packet, out var ctx, out var error))
                {
                    this.goToService.Execute(ctx!);
                }
                else if (error is not null)
                {
                    var response = new ResponsePacket(error, logger);
                    _ = response.SendAsync(player);
                }

                break;

#endif

            case PacketType.AbilityUse:
                var ability = packet.GetPayload<AbilityUsePayload>();
                var action = this.actionHandler.GetAbilityAction(ability.AbilityType, player.Instance);
                action?.Invoke();
                break;
        }
    }

    private void RegisterPlayerAction(PlayerConnection player)
    {
        lock (player)
        {
            player.HasMadeActionThisTick = true;
#if HACKATHON
            onHackathonBotMadeAction?.Invoke(player);
#endif
        }
    }

#if HACKATHON

    private bool ValidateGameStateForBot(PlayerConnection player, Packet packet)
    {
        if (!player.IsHackathonBot)
        {
            return true;
        }

        var gameStateIdProperty = JsonNamingPolicy.CamelCase.ConvertName(nameof(ActionPayload.GameStateId));
        var receivedId = (string?)packet.Payload[gameStateIdProperty];

        lock (game.GameManager.CurrentGameStateId!)
        {
            var isCurrent = receivedId is not null && game.GameManager.CurrentGameStateId == receivedId;

            if (receivedId is null)
            {
                this.SendWarning(player, "GameStateId is missing in the payload");
                return false;
            }

            if (!isCurrent)
            {
                this.SendWarning(player, PacketType.SlowResponseWarning);
                return false;
            }

            player.HasMadeActionToCurrentGameState = true;
            return true;
        }
    }

#endif

    private void SendWarning(PlayerConnection player, string message)
    {
        logger.Verbose(message + " ({player})", player);
        var payload = new CustomWarningPayload(message);
        _ = new ResponsePacket(payload, logger).SendAsync(player);
    }

    private void SendWarning(PlayerConnection player, PacketType type)
    {
        var payload = new EmptyPayload { Type = type };
        _ = new ResponsePacket(payload, logger).SendAsync(player);
    }
}
