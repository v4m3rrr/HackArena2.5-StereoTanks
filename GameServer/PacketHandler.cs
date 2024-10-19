using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameLogic;
using GameLogic.Networking;
using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents the packet handler.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="log">The logger.</param>
internal class PacketHandler(GameInstance game, Logger log)
{
#if HACKATHON

    /// <summary>
    /// Occurs when a bot made an action.
    /// </summary>
    public event EventHandler<PlayerConnection>? HackathonBotMadeAction;

    /// <summary>
    /// Gets the hackathon bot actions.
    /// </summary>
    public ConcurrentDictionary<PlayerConnection, Action> HackathonBotActions { get; } = new();

#endif

    /// <summary>
    /// Handles the connection.
    /// </summary>
    /// <param name="connection">The connection to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleConnection(Connection connection)
    {
        while (connection.Socket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 32];
            WebSocketReceiveResult result;
            try
            {
                result = await connection.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                log.Error(ex, "Receiving a message failed. ({connection})", connection);
                break;
            }

            if (!result.EndOfMessage)
            {
                log.Warning("Received message is too big. ({connection})", connection);
                log.Warning("Closing the connection with MessageTooBig status.");

                await connection.CloseAsync(
                    WebSocketCloseStatus.MessageTooBig,
                    "Message too big",
                    CancellationToken.None);

                game.RemoveConnection(connection.Socket);

                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                log.Verbose("Received message from {connection}:\n{message}", connection, Encoding.UTF8.GetString(buffer));
                this.HandleBuffer(connection, buffer);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                log.Verbose("Received close message from {connection}.", connection);
                await connection.CloseAsync();
                game.RemoveConnection(connection.Socket);
            }
        }
    }

    private async void ResponseWithInvalidPayload(Connection connection, Exception exception)
    {
        var type = PacketType.InvalidPayloadError;

        if (exception is not null)
        {
            type |= PacketType.HasPayload;
        }

        log.Verbose(
            "Client sent an invalid payload - {exception}. " +
            "Sending warning packet. ({player})",
            exception,
            connection);

        var sb = new StringBuilder()
            .AppendLine("Invalid payload:")
            .AppendLine(exception?.Message)
            .AppendLine("Action ignored.");

        var payload = new ErrorPayload(type, sb.ToString());
        var responsePacket = new ResponsePacket(payload, log);
        await responsePacket.SendAsync(connection);
    }

    private void HandleBuffer(Connection connection, byte[] buffer)
    {
        Packet packet;

        try
        {
            packet = PacketSerializer.Deserialize(buffer);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Packet deserialization failed. ({connection})", connection);
            return;
        }

        bool isHandled = false;

        if (connection is PlayerConnection player)
        {
            isHandled = this.HandlePlayerPacket(player, packet);
        }
        else if (connection is SpectatorConnection spectator)
        {
            isHandled = this.HandleSpectatorPacket(spectator, packet);
        }

#if DEBUG
        isHandled |= this.HandleDebugPacket(connection, packet);
#endif

        isHandled |= this.HandleOtherPacket(connection, packet);

        if (!isHandled)
        {
            log.Warning("Packet type ({type}) cannot be handled. ({connection})", packet.Type, connection);

            var payload = new ErrorPayload(
                PacketType.InvalidPacketTypeError | PacketType.HasPayload,
                $"Packet type ({packet.Type}) cannot be handled");

            var responsePacket = new ResponsePacket(payload, log);
            _ = responsePacket.SendAsync(connection);
        }
    }

    private bool HandlePlayerPacket(PlayerConnection player, Packet packet)
    {
        if (packet.Type.IsGroup(PacketType.PlayerResponseActionGroup))
        {
            try
            {
                this.HandlePlayerActionPacket(player, packet);
            }
            catch (Exception e)
            {
                log.Error(e, "Handling player response action packet failed. ({player})", player);
                throw;
            }

            return true;
        }

        return false;
    }

    private bool HandleSpectatorPacket(SpectatorConnection spectator, Packet packet)
    {
        return false;
    }

#if DEBUG

    private bool HandleDebugPacket(Connection connection, Packet packet)
    {
        if (packet.Type is PacketType.GlobalAbilityUse)
        {
            var payload = packet.GetPayload<AbilityUsePayload>();

            log.Debug(
                "Global ability use (type) packet received from {connection}.",
                payload.AbilityType,
                connection);

            foreach (var player in game.Players)
            {
                var action = this.GetAbilityAction(payload.AbilityType, player.Instance);
                action();
            }

            return true;
        }

        if (packet.Type is PacketType.GiveSecondaryItem)
        {
            var player = game.Players.First(p => p.Socket == connection.Socket);
            var payload = packet.GetPayload<GiveSecondaryItemPayload>();

            log.Debug(
                "Secondary item '{item}' given to {player} by {connection}.",
                payload.Item,
                player,
                connection);

            player.Instance.Tank.SecondaryItemType = payload.Item;
            return true;
        }

        if (packet.Type is PacketType.GlobalGiveSecondaryItem)
        {
            var payload = packet.GetPayload<GlobalGiveSecondaryItemPayload>();

            log.Debug(
                "Secondary item '{item}' given to all players by {connection}.",
                payload.Item,
                connection);

            foreach (var player in game.Players)
            {
                player.Instance.Tank.SecondaryItemType = payload.Item;
            }

            return true;
        }

        if (packet.Type is PacketType.ForceEndGame)
        {
            if (game.GameManager.Status is not GameStatus.Running)
            {
                var payload = new ErrorPayload(
                    PacketType.InvalidPacketUsageError | PacketType.HasPayload,
                    "Cannot force end the game when it is not running");

                var responsePacket = new ResponsePacket(payload, log);
                _ = responsePacket.SendAsync(connection);
                return true;
            }

            log.Debug("End game forced by {connection}.", connection);
            game.GameManager.EndGame();
            return true;
        }

        if (packet.Type is PacketType.SetPlayerScore)
        {
            var payload = packet.GetPayload<SetPlayerScorePayload>();
            var player = game.Players.First(
                p => p.Instance.Nickname.Equals(payload.PlayerNick, StringComparison.OrdinalIgnoreCase));

            if (player is null)
            {
                var errorPayload = new ErrorPayload(
                    PacketType.InvalidPacketUsageError | PacketType.HasPayload,
                    $"Player with nickname '{payload.PlayerNick}' not found");

                var responsePacket = new ResponsePacket(errorPayload, log);
                _ = responsePacket.SendAsync(connection);
                return true;
            }

            var scoreProperty = player.Instance.GetType().GetProperty(nameof(GameLogic.Player.Score));
            scoreProperty!.SetValue(player.Instance, payload.Score);

            log.Debug("Score '{player}' set to {score}.", player.Instance.Nickname, payload.Score);

            return true;
        }

        return false;
    }

#endif

    private bool HandleOtherPacket(Connection connection, Packet packet)
    {
        if (packet.Type == PacketType.Pong)
        {
            if (connection.Socket.State is not WebSocketState.Open)
            {
                return true;
            }

            connection.HasSentPong = true;
            log.Verbose("Received pong from {connection}.", connection);

            if (connection is PlayerConnection player)
            {
                player.Instance.Ping = (int)(DateTime.UtcNow - player.LastPingSentTime)!.TotalMilliseconds;
            }

            if (connection.IsSecondPingAttempt)
            {
                connection.IsSecondPingAttempt = false;
                log.Information("Client responded to the second ping. ({connection})", connection);
            }

            return true;
        }

        if (packet.Type == PacketType.LobbyDataRequest)
        {
            log.Verbose("Lobby data requested by {connection}.", connection);
            _ = game.LobbyManager.SendLobbyDataTo(connection);
            return true;
        }

        if (packet.Type == PacketType.GameStatusRequest)
        {
            log.Verbose("Game status requested by {connection}.", connection);
            var payload = game.PayloadHelper.GetGameStatusPayload();
            var responsePacket = new ResponsePacket(payload, log);
            _ = responsePacket.SendAsync(connection);
            return true;
        }

        if (packet.Type == PacketType.ReadyToReceiveGameState)
        {
            log.Debug("Client is ready to receive game state ({connection}).", connection);
            connection.IsReadyToReceiveGameState = true;
            return true;
        }

        return false;
    }

    private void HandlePlayerActionPacket(PlayerConnection player, Packet packet)
    {
#if HACKATHON

        bool? responsedToCurrentGameState = null;

        if (player.IsHackathonBot)
        {
            var gameStateIdPropertyName = JsonNamingPolicy.CamelCase.ConvertName(nameof(IActionPayload.GameStateId));
            var responseGameStateId = (string?)packet.Payload[gameStateIdPropertyName];

            lock (game.GameManager.CurrentGameStateId!)
            {
                responsedToCurrentGameState = responseGameStateId is not null
                    && game.GameManager.CurrentGameStateId == responseGameStateId;
            }

            if (responseGameStateId is null)
            {
                log.Verbose("GameStateId is missing in the payload. Sending warning packet. ({player})", player);
                var payload = new CustomWarningPayload("GameStateId is missing in the payload");
                var responsePacket = new ResponsePacket(payload, log);
                _ = responsePacket.SendAsync(player);
            }
            else if (!(bool)responsedToCurrentGameState)
            {
                log.Verbose("Player responded to an outdated game state. Sending warning packet. ({player})", player);
                var payload = new EmptyPayload() { Type = PacketType.SlowResponseWarning };
                var responsePacket = new ResponsePacket(payload, log);
                _ = responsePacket.SendAsync(player);
            }
        }

#endif

        lock (player)
        {
            if (player.HasMadeActionThisTick)
            {
                log.Verbose("Player already made an action in this tick. Sending warning packet. ({player})", player);
                var payload = new EmptyPayload() { Type = PacketType.PlayerAlreadyMadeActionWarning };
                var responsePacket = new ResponsePacket(payload, log);
                _ = responsePacket.SendAsync(player);
                return;
            }
        }

        if (player.Instance.IsDead && packet.Type is not PacketType.Pass)
        {
            log.Verbose("Player is dead. Sending warning packet. ({player})", player);
            var payload = new EmptyPayload() { Type = PacketType.ActionIgnoredDueToDeadWarning };
            var responsePacket = new ResponsePacket(payload, log);
            _ = responsePacket.SendAsync(player);
        }
        else
        {
            switch (packet.Type)
            {
                case PacketType.Movement:
                    this.HandleMovement(player, packet);
                    break;

                case PacketType.Rotation:
                    this.HandleRotation(player, packet);
                    break;

                case PacketType.AbilityUse:
                    this.HandleAbilityUse(player, packet);
                    break;

                case PacketType.Pass:
                    break;

                default:
                    log.Warning("Packet type '{type}' cannot be handled. ({player})", packet.Type, player);
                    return;
            }
        }

        lock (player)
        {
            player.HasMadeActionThisTick = true;
#if HACKATHON
            if (responsedToCurrentGameState is not null)
            {
                player.HasMadeActionToCurrentGameState = (bool)responsedToCurrentGameState;
            }
#endif
        }

#if HACKATHON
        this.HackathonBotMadeAction?.Invoke(this, player);
#endif
    }

    private void HandleMovement(PlayerConnection player, Packet packet)
    {
        var movement = packet.GetPayload<MovementPayload>(out var exception);

        if (exception is not null)
        {
            this.ResponseWithInvalidPayload(player, exception);
            return;
        }

        void Action()
        {
            log.Verbose("Trying to move the tank of {player} {direction}.", player, movement.Direction);
            game.Grid.TryMoveTank(player.Instance.Tank, movement.Direction);
        }

#if HACKATHON
        if (player.IsHackathonBot)
        {
            this.HackathonBotActions[player] = Action;
        }
        else
        {
#endif
            lock (game.Grid)
            {
                Action();
            }
#if HACKATHON
        }
#endif
    }

    private void HandleRotation(PlayerConnection player, Packet packet)
    {
        var rotation = packet.GetPayload<RotationPayload>(out var exception);

        if (exception is not null)
        {
            this.ResponseWithInvalidPayload(player, exception);
            return;
        }

        var actions = new List<Action>();

        if (rotation.TankRotation is { } tankRotation)
        {
            actions.Add(() =>
            {
                log.Verbose("Rotating the tank of {player} {rotation}.", player, tankRotation);
                player.Instance.Tank.Rotate(tankRotation);
            });
        }

        if (rotation.TurretRotation is { } turretRotation)
        {
            actions.Add(() =>
            {
                log.Verbose("Rotating the turret of {player} {rotation}.", player, turretRotation);
                player.Instance.Tank.Turret.Rotate(turretRotation);
            });
        }
#if HACKATHON
        if (player.IsHackathonBot)
        {
            this.HackathonBotActions[player] = () =>
            {
                foreach (var action in actions)
                {
                    action();
                }
            };
        }
        else
        {
#endif
            foreach (var action in actions)
            {
                action();
            }
#if HACKATHON
        }
#endif
    }

    private void HandleAbilityUse(PlayerConnection player, Packet packet)
    {
        var payload = packet.GetPayload<AbilityUsePayload>(out var exception);

        if (exception is not null)
        {
            this.ResponseWithInvalidPayload(player, exception);
            return;
        }

        // TODO: Add verbose logs for ability use
        var action = this.GetAbilityAction(payload.AbilityType, player.Instance);

#if HACKATHON
        if (player.IsHackathonBot)
        {
            this.HackathonBotActions[player] = () =>
            {
                lock (game.Grid)
                {
                    action();
                }
            };
        }
        else
        {
#endif
            lock (game.Grid)
            {
                action();
            }
#if HACKATHON
        }
#endif
    }

    private Func<dynamic?> GetAbilityAction(AbilityType type, Player player)
    {
        return type switch
        {
            AbilityType.FireBullet => player.Tank.Turret.TryFireBullet,
            AbilityType.FireDoubleBullet => player.Tank.Turret.TryFireDoubleBullet,
            AbilityType.UseLaser => () => player.Tank.Turret.TryUseLaser(game.Grid.WallGrid),
            AbilityType.UseRadar => () => player.Tank.TryUseRadar(),
            AbilityType.DropMine => player.Tank.TryDropMine,
            _ => throw new NotImplementedException($"Ability type '{type}' is not implemented"),
        };
    }
}
