using System.Net.WebSockets;
using GameLogic.Networking;
using Newtonsoft.Json;
using Serilog;

namespace GameServer;

/// <summary>
/// Represents a response packet.
/// </summary>
/// <param name="Payload">The payload of the response packet.</param>
/// <param name="Logger">The logger.</param>
/// <param name="Converters">The JSON converters to use when serializing the payload.</param>
internal record class ResponsePacket(
    IPacketPayload Payload,
    ILogger Logger,
    List<JsonConverter>? Converters = null)
{
    /// <summary>
    /// Gets the buffer of the serialized packet.
    /// </summary>
    /// <value>
    /// A byte array representing the serialized packet
    /// or <see langword="null"/> if the packet has not been sent yet.
    /// </value>
    public byte[]? Buffer { get; private set; }

    /// <summary>
    /// Sends the packet to a client.
    /// </summary>
    /// <param name="connection">The connection to send the packet to.</param>
    /// <param name="serializationLock">The lock to use when serializing the payload.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendAsync(Connection connection, object? serializationLock = null)
    {
        await this.SendAsync(connection, CancellationToken.None, serializationLock);
    }

    /// <summary>
    /// Sends the packet to a client.
    /// </summary>
    /// <param name="connection">The connection to send the packet to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="serializationLock">The lock to use when serializing the payload.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendAsync(Connection connection, CancellationToken cancellationToken, object? serializationLock = null)
    {
        var options = GetSerializationOptions(connection);

        if (serializationLock is not null)
        {
            lock (serializationLock)
            {
                this.Buffer = PacketSerializer.ToByteArray(this.Payload, this.Converters ?? [], options);
            }
        }
        else
        {
            this.Buffer = PacketSerializer.ToByteArray(this.Payload, this.Converters ?? [], options);
        }

        await connection.SendPacketSemaphore.WaitAsync(CancellationToken.None);

        try
        {
            if (connection.Socket.State is not WebSocketState.Open)
            {
                this.Logger.Verbose(
                    "Skipping sending packet ({payload}) to (status) connection: {connection}.",
                    this.Payload,
                    connection,
                    connection.Socket.State);
                return;
            }

#if DEBUG
            var packet = PacketSerializer.Deserialize(this.Buffer);
            PacketLogger.LogSent(connection, packet);
#endif

            await connection.Socket.SendAsync(
                new ArraySegment<byte>(this.Buffer),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            this.Logger.Error(
                ex,
                "Operation canceled while sending a {payload} packet. ({connection})",
                this.Payload,
                connection);
        }
        catch (Exception ex)
        {
            this.Logger.Error(
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
