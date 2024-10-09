using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using GameLogic;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the packet handler.
/// </summary>
/// <param name="game">The game instance.</param>
internal class PacketHandler(GameInstance game)
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
                Console.WriteLine("[ERROR] Receiving a message failed: ");
                Console.WriteLine("[^^^^^] {0}", ex.Message);
                Console.WriteLine("[^^^^^] Closing the connection with InternalServerError.");

                await connection.CloseAsync(
                    WebSocketCloseStatus.InternalServerError,
                    "Internal server error",
                    CancellationToken.None);

                break;
            }

            if (!result.EndOfMessage)
            {
                Console.WriteLine("[WARN] Received message is too big");
                Console.WriteLine("[^^^^] Closing the connection with MessageTooBig.");

                await connection.CloseAsync(
                    WebSocketCloseStatus.MessageTooBig,
                    "Message too big",
                    CancellationToken.None);

                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                this.HandleBuffer(connection, buffer);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await connection.CloseAsync();
                game.RemoveConnection(connection.Socket);
            }
        }
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
            Console.WriteLine("[ERROR] Packet deserialization failed: ");
            Console.WriteLine("[^^^^^] Sender: {0}", connection);
            Console.WriteLine("[^^^^^] Message: {0}", ex.Message);
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

        if (!isHandled)
        {
            Console.WriteLine($"[WARN] Packet type ({packet.Type}) cannot be handled.");
            Console.WriteLine($"[^^^^] Sender: {connection}");

            var payload = new ErrorPayload(
                PacketType.InvalidPacketTypeError | PacketType.HasPayload,
                $"Packet type ({packet.Type}) cannot be handled");

            var responsePacket = new ResponsePacket(payload);
            _ = responsePacket.SendAsync(connection);
        }
    }

    private bool HandlePlayerPacket(PlayerConnection player, Packet packet)
    {
        if (packet.Type == PacketType.Pong)
        {
            player.HasSentPong = true;
            player.Instance.Ping = (int)(DateTime.UtcNow - player.LastPingSentTime)!.TotalMilliseconds;
            return true;
        }

        if (packet.Type.IsGroup(PacketType.PlayerResponseActionGroup))
        {
            try
            {
                this.HandlePlayerActionPacket(player, packet);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Handling player response action packet failed: ");
                Console.WriteLine("[^^^^^] Player: {0}", player);
                Console.WriteLine("[^^^^^] Message: {0}", e.Message);
                throw;
            }

            return true;
        }

        return false;
    }

    private bool HandleSpectatorPacket(SpectatorConnection spectator, Packet packet)
    {
        if (packet.Type == PacketType.Pong)
        {
            spectator.HasSentPong = true;
            return true;
        }

        return false;
    }

#if DEBUG

    private bool HandleDebugPacket(Connection connection, Packet packet)
    {
        if (packet.Type is PacketType.ForceEndGame)
        {
            if (game.GameManager.Status is not GameStatus.Running)
            {
                var payload = new ErrorPayload(
                    PacketType.InvalidPacketUsageError | PacketType.HasPayload,
                    "Cannot force end the game when it is not running");

                var responsePacket = new ResponsePacket(payload);
                _ = responsePacket.SendAsync(connection);
                return true;
            }

            Console.WriteLine($"[DEBUG] End game forced by: {connection}");
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

                var responsePacket = new ResponsePacket(errorPayload);
                _ = responsePacket.SendAsync(connection);
                return true;
            }

            var scoreProperty = player.Instance.GetType().GetProperty(nameof(GameLogic.Player.Score));
            scoreProperty!.SetValue(player.Instance, payload.Score);

            Console.WriteLine($"[DEBUG] Score '{player.Instance.Nickname}' set to {payload.Score}");
            Console.WriteLine($" [^^^^] by: {connection}");

            return true;
        }

        return false;
    }

#endif

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
                var payload = new CustomWarningPayload("GameStateId is missing in the payload");
                var responsePacket = new ResponsePacket(payload);
                _ = responsePacket.SendAsync(player);
            }
            else if (!(bool)responsedToCurrentGameState)
            {
                var payload = new CustomWarningPayload("GameStateId is not the current game state id");
                var responsePacket = new ResponsePacket(payload);
                _ = responsePacket.SendAsync(player);
            }
        }

#endif

        lock (player)
        {
            if (player.HasMadeActionThisTick)
            {
                var payload = new EmptyPayload() { Type = PacketType.PlayerAlreadyMadeActionWarning };
                var responsePacket = new ResponsePacket(payload);
                _ = responsePacket.SendAsync(player);
                return;
            }
        }

        if (player.Instance.IsDead && packet.Type is not PacketType.Pass)
        {
            var payload = new EmptyPayload() { Type = PacketType.ActionIgnoredDueToDeadWarning };
            var responsePacket = new ResponsePacket(payload);
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
                    Console.WriteLine($"[WARN] Packet type '{packet.Type}' cannot be handled.");
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
        var movement = packet.GetPayload<MovementPayload>();
        void Action() => game.Grid.TryMoveTank(player.Instance.Tank, movement.Direction);

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
        var rotation = packet.GetPayload<RotationPayload>();
        var actions = new List<Action>();

        if (rotation.TankRotation is { } tankRotation)
        {
            actions.Add(() => player.Instance.Tank.Rotate(tankRotation));
        }

        if (rotation.TurretRotation is { } turretRotation)
        {
            actions.Add(() => player.Instance.Tank.Turret.Rotate(turretRotation));
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
        var payload = packet.GetPayload<AbilityUsePayload>();

        if (payload.AbilityType is not AbilityType.FireBullet)
        {
            throw new NotImplementedException("Ability type is not implemented");
        }

        Bullet? Action() => player.Instance.Tank.Turret.TryShoot();

#if HACKATHON
        if (player.IsHackathonBot)
        {
            this.HackathonBotActions[player] = () =>
            {
                lock (game.Grid)
                {
                    Action();
                }
            };
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
}
