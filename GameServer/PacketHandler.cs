using System.Diagnostics;
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
                if (game.SpectatorManager.IsSpectator(socket))
                {
                    game.SpectatorManager.RemoveSpectator(socket);
                }
                else
                {
                    game.PlayerManager.RemovePlayer(socket);
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
            isHandled = await this.HandlePlayerPacket(socket, packet);
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

    private async Task<bool> HandlePlayerPacket(WebSocket socket, Packet packet)
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
                await this.HandlePlayerActionPacket(socket, player, packet);
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

    private async Task HandlePlayerActionPacket(WebSocket socket, Player player, Packet packet)
    {
        bool responsedToCurrentGameState;

#if HACKATHON

        if (player.IsHackathonBot)
        {
            var responseGameStateId = (string?)packet.Payload[JsonNamingPolicy.CamelCase.ConvertName(nameof(IActionPayload.GameStateId))];
            Debug.WriteLine(game.GameManager.CurrentGameStateId + " " + responseGameStateId);

            lock (game.GameManager.CurrentGameStateId!)
            {
                responsedToCurrentGameState = responseGameStateId is not null
                    && game.GameManager.CurrentGameStateId == responseGameStateId;
            }

            lock (player)
            {
                player.HasMadeActionToCurrentGameState = responsedToCurrentGameState;
            }

            if (responseGameStateId is null)
            {
                _ = game.SendPlayerPacketAsync(socket, new EmptyPayload() { Type = PacketType.MissingGameStateIdWarning });
            }
            else if (!responsedToCurrentGameState)
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

            player.HasMadeActionThisTick = true;
        }

#if HACKATHON
        this.HackathonBotMadeAction?.Invoke(this, player);
#endif

        switch (packet.Type)
        {
            case PacketType.TankMovement:
                var movement = packet.GetPayload<TankMovementPayload>();
                game.Grid.TryMoveTank(player.Instance.Tank, movement.Direction);
                break;

            case PacketType.TankRotation:
                var rotation = packet.GetPayload<TankRotationPayload>();
                if (rotation.TankRotation is { } tankRotation)
                {
                    player.Instance.Tank.Rotate(tankRotation);
                }

                if (rotation.TurretRotation is { } turretRotation)
                {
                    player.Instance.Tank.Turret.Rotate(turretRotation);
                }

                break;

            case PacketType.TankShoot:
                _ = player.Instance.Tank.Turret.TryShoot();
                break;

            default:
                Console.WriteLine($"Invalid packet type ({packet.Type}) in PlayerResponseGroup");
                return;
        }
    }
}
