using System.Net.WebSockets;
using GameLogic.Networking;
using Newtonsoft.Json;

namespace GameServer;

/// <summary>
/// Represents a response packet.
/// </summary>
/// <param name="Payload">The payload of the response packet.</param>
/// <param name="Converters">The JSON converters to use when serializing the payload.</param>
internal record class ResponsePacket(IPacketPayload Payload, List<JsonConverter>? Converters = null)
{
    /// <summary>
    /// Sends the packet to a client.
    /// </summary>
    /// <param name="connection">The connection to send the packet to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendAsync(Connection connection)
    {
        var options = this.GetSerializationOptions(connection);
        var buffer = PacketSerializer.ToByteArray(this.Payload, this.Converters ?? [], options);

        await connection.SendPacketSemaphore.WaitAsync();

        try
        {
            await connection.Socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR WHILE SENDING PACKET (Packet.SendAsync): " + e.Message);
        }
        finally
        {
            _ = connection.SendPacketSemaphore.Release();
        }
    }

    private SerializationOptions GetSerializationOptions(Connection connection)
    {
        return connection is not PlayerConnection playerConnection
            ? SerializationOptions.Default
            : new SerializationOptions()
            {
                TypeOfPacketType = playerConnection.Data.TypeOfPacketType,
            };
    }
}
