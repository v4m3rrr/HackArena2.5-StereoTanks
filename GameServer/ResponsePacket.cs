using System.Net.WebSockets;
using GameLogic.Networking;
using Newtonsoft.Json;
using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents a response packet.
/// </summary>
/// <param name="Payload">The payload of the response packet.</param>
/// <param name="Log">The logger.</param>
/// <param name="Converters">The JSON converters to use when serializing the payload.</param>
internal record class ResponsePacket(IPacketPayload Payload, Logger Log, List<JsonConverter>? Converters = null)
{
    /// <summary>
    /// Sends the packet to a client.
    /// </summary>
    /// <param name="connection">The connection to send the packet to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendAsync(Connection connection)
    {
        await this.SendAsync(connection, CancellationToken.None);
    }

    /// <summary>
    /// Sends the packet to a client.
    /// </summary>
    /// <param name="connection">The connection to send the packet to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendAsync(Connection connection, CancellationToken cancellationToken)
    {
        var options = GetSerializationOptions(connection);
        var buffer = PacketSerializer.ToByteArray(this.Payload, this.Converters ?? [], options);

        await connection.SendPacketSemaphore.WaitAsync(CancellationToken.None);

        try
        {
            if (connection.Socket.State is not WebSocketState.Open)
            {
                this.Log.Verbose(
                    "Skipping sending packet ({payload}) to (status) connection: {connection}.",
                    this.Payload,
                    connection,
                    connection.Socket.State);
                return;
            }

            await connection.Socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            this.Log.Error(
                ex,
                "Operation canceled while sending a {payload} packet. ({connection})",
                this.Payload,
                connection);
        }
        catch (Exception ex)
        {
            this.Log.Error(
                ex,
                "Error while sending a {payload} packet. ({connection})",
                this.Payload,
                connection);
        }
        finally
        {
            _ = connection.SendPacketSemaphore.Release();
        }
    }

    private static SerializationOptions GetSerializationOptions(Connection connection)
    {
        return new SerializationOptions()
        {
            EnumSerialization = connection.Data.EnumSerialization,
        };
    }
}
