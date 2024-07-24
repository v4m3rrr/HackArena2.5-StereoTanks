using System.Net.WebSockets;
using GameLogic;
using GameLogic.Networking;

namespace GameServer;

internal class GameInstance(string id)
{
    private readonly Dictionary<WebSocket, DateTime> lastActionTime = [];

    public static float BroadcastInterval { get; } = 0.1f;

    public string Id { get; } = id;

    public string JoinCode { get; init; } = id[^4..];

    public Grid Grid { get; } = new Grid();

    public Dictionary<WebSocket, Tank> ClientTanks { get; } = [];

    public void AddClient(WebSocket socket)
    {
        var tank = this.Grid.GenerateTank();
        this.ClientTanks.Add(socket, tank);
        this.lastActionTime.Add(socket, DateTime.MinValue);
    }

    public void RemoveClient(WebSocket socket)
    {
        var tank = this.ClientTanks.Remove(socket);
        /* Grid.RemoveTank(tank); TODO */
        _ = this.lastActionTime.Remove(socket);
    }

    public async Task BroadcastGridState()
    {
        var packet = this.Grid.ToPayload();
        await this.Broadcast(packet);
    }

    public async Task HandleBuffer(WebSocket socket, byte[] buffer)
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

        switch (packet.Type)
        {
            case PacketType.TankMovement:
                var movement = packet.GetPayload<TankMovementPayload>();
                this.Grid.TryMoveTank(this.ClientTanks[socket], movement.Direction);
                break;

            case PacketType.TankRotation:
                var rotation = packet.GetPayload<TankRotationPayload>();
                if (rotation.TankRotation is { } tankRotation)
                {
                    this.ClientTanks[socket].Rotate(tankRotation);
                }

                if (rotation.TurretRotation is { } turretRotation)
                {
                    this.ClientTanks[socket].Turret.Rotate(turretRotation);
                }

                break;

#if DEBUG
            case PacketType.ShootAll:
                foreach (var tank in this.ClientTanks.Values)
                {
                    tank.Shoot();
                }

                break;
#endif

            case PacketType.TankShoot:
                _ = this.ClientTanks[socket].Shoot();
                break;

            case PacketType.GameData:
                var response = new GameStatePayload(this.Id, this.JoinCode, BroadcastInterval);
                await SendPacketAsync(socket, response);
                break;

            case PacketType.Ping:
                await SendPacketAsync(socket, new EmptyPayload() { Type = PacketType.Ping });
                break;
        }
    }

    private static async Task SendPacketAsync(WebSocket socket, IPacketPayload packet)
    {
        var buffer = PacketSerializer.ToByteArray(packet);

        Monitor.Enter(socket);
        try
        {
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE SENDING PACKET (SendPacketAsync): " + e.Message);
        }
        finally
        {
            Monitor.Exit(socket);
        }
    }

    private async Task Broadcast(IPacketPayload packet)
    {
        var buffer = PacketSerializer.ToByteArray(packet);

        foreach (var client in this.ClientTanks.Keys)
        {
            if (client.State == WebSocketState.Open)
            {
                Monitor.Enter(client);
                try
                {
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR WHILE BROADCASTING PACKET: " + e.Message);
                }
                finally
                {
                    Monitor.Exit(client);
                }
            }
        }
    }
}
