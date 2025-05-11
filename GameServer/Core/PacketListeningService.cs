using System.Net.WebSockets;
using System.Text;
using Serilog;

namespace GameServer.System;

/// <summary>
/// Provides a service that listens for incoming packets from a connection.
/// </summary>
internal static class PacketListeningService
{
    private const int BufferSize = 1024 * 32;

    /// <summary>
    /// Starts listening for packets from the specified connection.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <param name="connection">The connection to listen to.</param>
    /// <param name="handler">The packet handler to process incoming packets.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="cancellationToken">The cancellation token to stop listening.</param>
    /// <returns>A task representing the receive loop.</returns>
    public static async Task StartReceivingAsync(
        GameInstance game,
        Connection connection,
        PacketHandler handler,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        WebSocket socket = connection.Socket;

        while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            byte[] buffer = new byte[BufferSize];

            try
            {
                result = await connection.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (!result.EndOfMessage)
                {
                    logger.Warning("Received message is too big. ({connection})", connection);
                    logger.Warning("Closing the connection with MessageTooBig status.");

                    await connection.CloseAsync(
                        WebSocketCloseStatus.MessageTooBig,
                        "Message too big",
                        CancellationToken.None);

                    game.RemoveConnection(connection.Socket);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    logger.Verbose("Received message from {connection}:\n{message}", connection, Encoding.UTF8.GetString(buffer));
                    _ = handler.HandleBufferAsync(connection, buffer);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    logger.Verbose("Received close message from {connection}.", connection);
                    await connection.CloseAsync(cancellationToken: cancellationToken);
                    game.RemoveConnection(connection.Socket);
                }
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Packet listening canceled. ({connection})", connection);
                break;
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Exception while receiving packet. Closing connection. ({connection})", connection);

                try
                {
                    await connection.CloseAsync(description: "ReceiveError", cancellationToken: CancellationToken.None);
                }
                catch
                {
                    logger.Error("Failed to close connection gracefully. Aborting socket. ({connection})", connection);
                    connection.Socket.Abort();
                }

                game?.RemoveConnection(connection.Socket);
                break;
            }
        }
    }
}
