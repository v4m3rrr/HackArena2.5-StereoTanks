using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
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
    public event EventHandler<Player>? HackathonBotMadeAction;

    /// <summary>
    /// Gets the hackathon bot actions.
    /// </summary>
    public ConcurrentDictionary<Player, Action> HackathonBotActions { get; } = new();

#endif

    /// <summary>
    /// Handles the connection.
    /// </summary>
    /// <param name="socket">The socket of the connection.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleConnection(WebSocket socket)
    {
        while (socket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 32];
            WebSocketReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (WebSocketException)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Internal server error", CancellationToken.None);
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                await this.HandleBuffer(socket, buffer);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                var isSpectator = game.SpectatorManager.IsSpectator(socket);

                if (isSpectator)
                {
                    game.SpectatorManager.RemoveSpectator(socket);
                    Console.WriteLine("Spectator disconnected");
                }
                else
                {
                    var player = game.PlayerManager.Players[socket];
                    game.PlayerManager.RemovePlayer(socket);
                    Console.WriteLine($"Player {player.Instance.Nickname} disconnected");
                }
            }
        }
    }

    private async Task HandleBuffer(WebSocket socket, byte[] buffer)
    {
        Packet packet;
        try
        {
            packet = PacketSerializer.Deserialize(buffer);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE DESERIALIZING PACKET (HandleBuffer): " + e.Message);
            return;
        }

        bool isHandled = false;

        if (!game.SpectatorManager.IsSpectator(socket))
        {
            isHandled = this.HandlePlayerPacket(socket, packet);
        }

        isHandled |= await this.HandleOtherPacket(socket, packet);

        if (!isHandled)
        {
            Console.WriteLine($"Invalid packet type ({packet.Type})");

            var payload = new ErrorPayload(
                PacketType.InvalidPacketTypeError,
                $"Packet type ({packet.Type}) cannot be handled");

            await game.SendPlayerPacketAsync(socket, payload);
        }
    }

    private bool HandlePlayerPacket(WebSocket socket, Packet packet)
    {
        Player player = game.PlayerManager.Players[socket];

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
                this.HandlePlayerActionPacket(socket, player, packet);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WHILE HANDLING PLAYER MOVEMENT PACKET: " + e.Message);
                throw;
            }

            return true;
        }

        return false;
    }

    private async Task<bool> HandleOtherPacket(WebSocket socket, Packet packet)
    {
#if DEBUG
        if (packet.Type is PacketType.ForceEndGame)
        {
            if (game.GameManager.Status is not GameStatus.Running)
            {
                var payload = new ErrorPayload(
                    PacketType.InvalidPacketUsageError,
                    "Cannot force end the game when it is not running");

                await game.SendPacketAsync(socket, payload);
            }

            game.GameManager.EndGame();

            Console.WriteLine("Forced end game");

            return true;
        }

        if (packet.Type is PacketType.SetPlayerScore)
        {
            var payload = packet.GetPayload<SetPlayerScorePayload>();
            var player = game.PlayerManager.Players.Values.FirstOrDefault(
                p => p.Instance.Nickname.Equals(payload.PlayerNick, StringComparison.OrdinalIgnoreCase));

            if (player is null)
            {
                var errorPayload = new ErrorPayload(
                    PacketType.InvalidPacketUsageError,
                    $"Player with nickname '{payload.PlayerNick}' not found");

                await game.SendPacketAsync(socket, errorPayload);
                return true;
            }

            var scoreProperty = player.Instance.GetType().GetProperty(nameof(GameLogic.Player.Score));
            scoreProperty!.SetValue(player.Instance, payload.Score);

            Console.WriteLine($"Set score of player '{player.Instance.Nickname}' to {payload.Score}");

            return true;
        }
#endif

        return false;
    }

    private void HandlePlayerActionPacket(WebSocket socket, Player player, Packet packet)
    {
#if HACKATHON

        bool? responsedToCurrentGameState = null;

        if (player.IsHackathonBot)
        {
            var responseGameStateId = (string?)packet.Payload[JsonNamingPolicy.CamelCase.ConvertName(nameof(IActionPayload.GameStateId))];

            lock (game.GameManager.CurrentGameStateId!)
            {
                responsedToCurrentGameState = responseGameStateId is not null
                    && game.GameManager.CurrentGameStateId == responseGameStateId;
            }

            if (responseGameStateId is null)
            {
                _ = game.SendPlayerPacketAsync(socket, new CustomWarningPayload("GameStateId is missing in the payload"));
            }
            else if (!(bool)responsedToCurrentGameState)
            {
                _ = game.SendPlayerPacketAsync(socket, new EmptyPayload() { Type = PacketType.SlowResponseWarning });
            }
        }

#endif

        lock (player)
        {
            if (player.HasMadeActionThisTick)
            {
                _ = game.SendPlayerPacketAsync(socket, new EmptyPayload() { Type = PacketType.PlayerAlreadyMadeActionWarning });
                return;
            }
        }

        switch (packet.Type)
        {
            case PacketType.TankMovement:
                this.HandleMoveTank(player, packet);
                break;

            case PacketType.TankRotation:
                this.HandleRotateTank(player, packet);
                break;

            case PacketType.TankShoot:
                this.HandleShootTank(player);
                break;

            case PacketType.ResponsePass:
                _ = game.SendPlayerPacketAsync(socket, new EmptyPayload() { Type = PacketType.ActionIgnoredDueToDeadWarning });
                break;

            default:
                Console.WriteLine($"Invalid packet type ({packet.Type}) in PlayerResponseGroup");
                return;
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

    private void HandleMoveTank(Player player, Packet packet)
    {
        var movement = packet.GetPayload<TankMovementPayload>();
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

    private void HandleRotateTank(Player player, Packet packet)
    {
        var rotation = packet.GetPayload<TankRotationPayload>();
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

    private void HandleShootTank(Player player)
    {
        GameLogic.Bullet? Action() => player.Instance.Tank.Turret.TryShoot();

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
