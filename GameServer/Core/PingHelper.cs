using System.Net.WebSockets;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Provides functionality for pinging clients and detecting disconnects.
/// </summary>
internal static class PingHelper
{
    private const int PingInterval = 1000;

    /// <summary>
    /// Starts the ping loop for a connection.
    /// </summary>
    /// <param name="connection">The connection to ping.</param>
    /// <param name="game">The game instance.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Start(Connection connection, GameInstance game, ILogger logger)
    {
        await Task.Delay(500);

        int pongTimeout = game.Options.NoPongTimeout;

        SendPing();
        connection.LastPingSentTime = DateTime.UtcNow;

        while (connection.Socket.State == WebSocketState.Open)
        {
            try
            {
                if (connection.HasSentPong)
                {
                    connection.HasSentPong = false;
                    SendPing();
                    connection.LastPingSentTime = DateTime.UtcNow;
                }
                else
                {
                    int timeout = pongTimeout * (connection.IsSecondPingAttempt ? 2 : 1);
                    if ((DateTime.UtcNow - connection.LastPingSentTime).TotalMilliseconds > timeout)
                    {
                        if (!connection.IsSecondPingAttempt)
                        {
                            logger.Warning("No pong in {timeout}ms. Sending second ping... ({connection})", pongTimeout, connection);
                            SendPing();
                            connection.IsSecondPingAttempt = true;
                        }
                        else
                        {
                            logger.Warning("No pong after second ping. Disconnecting client... ({connection})", connection);
                            await ForceCloseAsync();
                            game.RemoveConnection(connection.Socket);
                            return;
                        }
                    }
                }

                await Task.Delay(PingInterval);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Ping loop error ({connection})", connection);
                await Task.Delay(100);
            }
        }

        void SendPing()
        {
            logger.Verbose("Sending ping to {connection}", connection);
            var payload = new EmptyPayload { Type = PacketType.Ping };
            var packet = new ResponsePacket(payload, logger);
            var token = new CancellationTokenSource(pongTimeout).Token;
            _ = packet.SendAsync(connection, token);
        }

        async Task ForceCloseAsync()
        {
            var token = new CancellationTokenSource(1000).Token;
            try
            {
                await connection.CloseAsync(description: "NoPongResponse", cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                logger.Error("Failed to close cleanly. Aborting socket... ({connection})", connection);
                connection.Socket.Abort();
            }
        }
    }
}
