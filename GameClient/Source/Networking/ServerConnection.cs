using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameLogic.Networking;

namespace GameClient.Networking;

/// <summary>
/// Represents a connection to a WebSocket server.
/// </summary>
internal static class ServerConnection
{
    private static ClientWebSocket client = new();

    /// <summary>
    /// Occurs when a message is received from the server.
    /// </summary>
    public static event Action<WebSocketReceiveResult, string>? MessageReceived;

    /// <summary>
    /// Occurs when the connection to the server is connecting.
    /// </summary>
    public static event Action<string>? Connecting;

    /// <summary>
    /// Occurs when the connection to the server is established.
    /// </summary>
    /// <remarks>
    /// The connection is established when the WebSocket
    /// is connected and the connection is accepted.
    /// </remarks>
    public static event Action? Established;

    /// <summary>
    /// Occurs when an error occurs.
    /// </summary>
    public static event Action<string>? ErrorThrew;

    /// <summary>
    /// Gets or sets the buffer size for the WebSocket messages.
    /// </summary>
    public static int BufferSize { get; set; } = 1024 * 32;

    /// <summary>
    /// Gets or sets the timeout for the server connection in seconds.
    /// </summary>
    public static int ServerTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets a value indicating whether the WebSocket is connected.
    /// </summary>
    public static bool IsConnected => client.State == WebSocketState.Open;

    /// <summary>
    /// Gets a value indicating whether the connection is accepted.
    /// </summary>
    public static bool IsAccepted { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the connection is established.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the connection is established;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The connection is established when the WebSocket
    /// is connected and the connection is accepted.
    /// </remarks>
    public static bool IsEstablished => IsConnected && IsAccepted;

    /// <summary>
    /// Gets the connection data.
    /// </summary>
    public static ConnectionData Data { get; private set; }

    /// <summary>
    /// Connects to the specified server.
    /// </summary>
    /// <param name="data">The connection data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task<ConnectionStatus> ConnectAsync(ConnectionData data)
    {
        string serverUrl = data.GetServerUrl();

        Connecting?.Invoke(serverUrl);

        ConnectionStatus status;

        try
        {
            client = new ClientWebSocket();

            await client.ConnectAsync(new Uri(serverUrl), CancellationToken.None);

            status = await WaitForAccept();

            if (status is not ConnectionStatus.Success)
            {
                return status;
            }

            Data = data;
            IsAccepted = true;
            Established?.Invoke();

            _ = Task.Run(ReceiveMessages);
        }
        catch (WebSocketException ex)
        {
            if (ex.Message != "Unable to connect to the remote server")
            {
                ErrorThrew?.Invoke($"An error occurred while connecting to the WebSocket server: {ex.Message}");
            }

            return new ConnectionStatus.Failed(ex);
        }

        return status;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">The message to send to the server.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SendAsync(string message)
    {
        if (!IsEstablished)
        {
            ErrorThrew?.Invoke("WebSocket is not established.");
            return;
        }

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        var messageSegment = new ArraySegment<byte>(messageBytes);

        try
        {
            await client.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            ErrorThrew?.Invoke($"An error occurred while sending the message: {ex.Message}");
        }
    }

    /// <summary>
    /// Closes the WebSocket connection.
    /// </summary>
    /// <param name="description">The description of the closing.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task CloseAsync(string description = "Closing")
    {
        try
        {
            if (client.State == WebSocketState.Open)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, description, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            DebugConsole.ThrowError(ex);
        }
    }

    private static async Task<ConnectionStatus> WaitForAccept()
    {
        while (client.State == WebSocketState.Open)
        {
            byte[] buffer = new byte[BufferSize];
            WebSocketReceiveResult result;

            try
            {
                result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                ErrorThrew?.Invoke($"A WebSocket error occurred: {ex.Message}");
                break;
            }

            var packet = PacketSerializer.Deserialize(buffer);
            if (packet.Type == PacketType.ConnectionAccepted)
            {
                return new ConnectionStatus.Success();
            }
            else if (packet.Type == PacketType.ConnectionRejected)
            {
                await CloseAsync("Closing due to rejection");

                var payload = packet.GetPayload<ConnectionRejectedPayload>();
                return new ConnectionStatus.Rejected(payload.Reason);
            }
        }

        return new ConnectionStatus.Failed();
    }

    private static async Task ReceiveMessages()
    {
        while (client.State == WebSocketState.Open)
        {
            byte[] buffer = new byte[BufferSize];
            WebSocketReceiveResult result;

            try
            {
                result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                ErrorThrew?.Invoke($"A WebSocket error occurred: {ex.Message}");
                break;
            }

            if (result.CloseStatus.HasValue)
            {
                if (result.CloseStatus is WebSocketCloseStatus.NormalClosure)
                {
                    var msg = result.CloseStatusDescription is null
                        ? $"Server connection closed normally."
                        : $"Server connection closed normally - {result.CloseStatusDescription}";

                    DebugConsole.SendMessage(msg);
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                else
                {
                    DebugConsole.ThrowError(
                        $"Server connection closed unexpectedly (status: {result.CloseStatus}, " +
                        $"description: {result.CloseStatusDescription ?? "N/A"})");
                    break;
                }
            }

            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            MessageReceived?.Invoke(result, message);
        }
    }
}
