using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using GameLogic;
using GameLogic.Networking;
using GameServer.Enums;
using GameServer.Services;
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
                if (action is null)
                {
                    log.Warning("Ability type '{type}' is not valid.", payload.AbilityType);
                    return false;
                }

                action();
            }

            return true;
        }

#if STEREO

        if (packet.Type is PacketType.ChargeAbility)
        {
            var player = game.Players.First(p => p.Socket == connection.Socket);
            var payload = packet.GetPayload<ChargeAbilityPayload>();

            log.Debug(
                "Ability '{ability}' charged for {player} by {connection}.",
                payload.AbilityType,
                player,
                connection);

            player.Instance.Tank.ChargeAbility(payload.AbilityType);
            return true;
        }

#else
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

#endif

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

#if !STEREO

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

            var scoreProperty = player.Instance.GetType().GetProperty(nameof(Player.Score));
            scoreProperty!.SetValue(player.Instance, payload.Score);

            log.Debug("Score '{player}' set to {score}.", player.Instance.Nickname, payload.Score);

            return true;
        }

#endif

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
            var gameStateIdPropertyName = JsonNamingPolicy.CamelCase.ConvertName(nameof(ActionPayload.GameStateId));
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
#if HACKATHON && STEREO
                if (player.IsHackathonBot || packet.Type != PacketType.GoTo)
#elif STEREO
                if (packet.Type != PacketType.GoTo)
#endif
                {
                    log.Verbose("Hackathon bot already made an action in this tick. Sending warning packet. ({player})", player);
                    var payload = new EmptyPayload() { Type = PacketType.PlayerAlreadyMadeActionWarning };
                    var responsePacket = new ResponsePacket(payload, log);
                    _ = responsePacket.SendAsync(player);
                }

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

#if STEREO
                case PacketType.GoTo:
                    _ = this.HandleGoTo(player, packet);
                    break;
#endif

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
        var payload = packet.GetPayload<MovementPayload>(out var exception);

        if (exception is not null)
        {
            this.ResponseWithInvalidPayload(player, exception);
            return;
        }

        this.HandleMovement(player, payload.Direction);
    }

    private void HandleMovement(PlayerConnection player, MovementDirection direction)
    {
        void Action()
        {
            log.Verbose("Trying to move the tank of {player} {direction}.", player, direction);
            game.Grid.TryMoveTank(player.Instance.Tank, direction);
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
        var payload = packet.GetPayload<RotationPayload>(out var exception);

        if (exception is not null)
        {
            this.ResponseWithInvalidPayload(player, exception);
            return;
        }

        this.HandleRotation(player, payload.TankRotation, payload.TurretRotation);
    }

    private void HandleRotation(PlayerConnection player, Rotation? tankRotation, Rotation? turretRotation)
    {
        var actions = new List<Action>();

        if (tankRotation is not null)
        {
            actions.Add(() =>
            {
                log.Verbose("Rotating the tank of {player} {rotation}.", player, tankRotation);
                player.Instance.Tank.Rotate(tankRotation.Value);
            });
        }

        if (turretRotation is not null)
        {
            actions.Add(() =>
            {
                log.Verbose("Rotating the turret of {player} {rotation}.", player, turretRotation);
                player.Instance.Tank.Turret.Rotate(turretRotation.Value);
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

        if (action is null)
        {
            var responsePayload = new ErrorPayload(
                PacketType.InvalidPacketUsageErrorWithPayload,
                $"Ability type '{payload.AbilityType}' is not valid");

            var responsePacket = new ResponsePacket(responsePayload, log);
            _ = responsePacket.SendAsync(player);
            return;
        }

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

#if STEREO

    private async Task HandleGoTo(PlayerConnection player, Packet packet)
    {
        var payload = packet.GetPayload<GoToPayload>(out var exception);
        if (exception is not null)
        {
            this.ResponseWithInvalidPayload(player, exception);
            return;
        }

        if (player.LastSentGameStateBuffer is null)
        {
            log.Error("Player has no last sent game state buffer but sent a go to packet. ({player})", player);
            var responsePayload = new ErrorPayload(PacketType.InternalError, "No last sent game state");
            var responsePacket = new ResponsePacket(responsePayload, log);
            await responsePacket.SendAsync(player);
            return;
        }

        var context = new GameSerializationContext.Player(player.Instance, player.Data.EnumSerialization);
        var converters = GameStatePayload.GetConverters(context);
        var serializer = PacketSerializer.GetSerializer(converters);
        var gameStatePacket = PacketSerializer.Deserialize(player.LastSentGameStateBuffer);
        var gameState = gameStatePacket.GetPayload<GameStatePayload.ForPlayer>(serializer, out var gameStateException);

        if (gameStateException is not null)
        {
            log.Error(gameStateException, "Game state deserialization failed. ({player})", player);
        }

        var pathFinder = new PathFinder(game.Settings, gameState, player.Instance);
        var pathResult = pathFinder.GetNextAction(payload.X, payload.Y, payload.Costs, payload.Penalties);
        if (pathResult is null)
        {
            return;
        }

        switch (pathResult)
        {
            case PathAction.MoveForward:
                this.HandleMovement(player, MovementDirection.Forward);
                break;

            case PathAction.MoveBackward:
                this.HandleMovement(player, MovementDirection.Backward);
                break;

            case PathAction.RotateLeft:
                this.HandleRotation(player, tankRotation: Rotation.Left, payload.TurretRotation);
                break;

            case PathAction.RotateRight:
                this.HandleRotation(player, tankRotation: Rotation.Right, payload.TurretRotation);
                break;
        }
    }

#endif

#if STEREO
    private Func<dynamic?>? GetAbilityAction(AbilityType type, Player player)
#else
    private Func<dynamic?> GetAbilityAction(AbilityType type, Player player)
#endif
    {
        return type switch
        {
            AbilityType.FireBullet => player.Tank.Turret.TryFireBullet,
#if !STEREO
            AbilityType.FireDoubleBullet => player.Tank.Turret.TryFireDoubleBullet,
            AbilityType.UseLaser => () => player.Tank.Turret.TryUseLaser(game.Grid.WallGrid),
            AbilityType.UseRadar => () => player.Tank.TryUseRadar(),
            AbilityType.DropMine => player.Tank.TryDropMine,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid ability type"),
#else
            AbilityType.FireDoubleBullet when player.Tank is LightTank light => light.Turret.TryFireDoubleBullet,
            AbilityType.UseRadar when player.Tank is LightTank light => () => light.TryUseRadar(),
            AbilityType.UseLaser when player.Tank is HeavyTank heavy => () => heavy.Turret.TryUseLaser(game.Grid.WallGrid),
            AbilityType.DropMine when player.Tank is HeavyTank heavy => heavy.TryDropMine,
            _ => null,
#endif
        };
    }
}
