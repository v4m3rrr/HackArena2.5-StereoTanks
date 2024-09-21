using System.Net.WebSockets;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the packet handler.
/// </summary>
/// <param name="game">The game instance.</param>
internal class PacketHandler(GameInstance game)
{
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

        if (!game.SpectatorManager.IsSpectator(socket))
        {
            await this.HandlePlayerPacket(socket, packet);
        }
    }

    private async Task HandlePlayerPacket(WebSocket socket, Packet packet)
    {
        Player player = game.PlayerManager.Players[socket];

        if (packet.Type == PacketType.Pong)
        {
            player.HasSentPong = true;
            player.Instance.Ping = (int)(DateTime.UtcNow - player.LastPingSentTime)!.TotalMilliseconds;
            return;
        }

        if (packet.Type.IsGroup(PacketType.PlayerResponseGroup))
        {
            try
            {
                await this.HandlePlayerMovementPacket(socket, player, packet);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR WHILE HANDLING PLAYER MOVEMENT PACKET: " + e.Message);
                throw;
            }
        }
    }

    private async Task HandlePlayerMovementPacket(WebSocket socket, Player player, Packet packet)
    {
        Monitor.Enter(player);
        try
        {
            if (player.HasMadeMovementThisTick)
            {
                await game.SendPlayerPacketAsync(socket, new EmptyPayload() { Type = PacketType.AlreadyMadeMovement });
                return;
            }

            player.HasMadeMovementThisTick = true;
        }
        finally
        {
            Monitor.Exit(player);
        }

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
