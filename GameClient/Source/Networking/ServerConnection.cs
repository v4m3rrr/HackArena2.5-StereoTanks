using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    /// Occurs when the connection to the server is establishing.
    /// </summary>
    public static event Action<string>? Connecting;

    /// <summary>
    /// Occurs when the connection to the server is established.
    /// </summary>
    public static event Action? Connected;

    /// <summary>
    /// Occurs when an error occurs.
    /// </summary>
    public static event Action<string>? ErrorThrew;

    /// <summary>
    /// Gets or sets the buffer size for the WebSocket messages.
    /// </summary>
    public static int BufferSize { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the timeout for the server connection in seconds.
    /// </summary>
    public static int ServerTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets a value indicating whether the WebSocket is connected.
    /// </summary>
    public static bool IsConnected => client.State == WebSocketState.Open;

    /// <summary>
    /// Gets the connection data.
    /// </summary>
    public static ConnectionData Data { get; private set; }

    /// <summary>
    /// Connects to the specified server.
    /// </summary>
    /// <param name="data">The connection data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task<bool> ConnectAsync(ConnectionData data, int bufferSize)
    {
        BufferSize = bufferSize;

        string serverUrl = data.GetServerWsUrl();

        Connecting?.Invoke(serverUrl);

        if (!await ValidateServerConnectionAsync(data))
        {
            return false;
        }

        try
        {
            client = new ClientWebSocket();
            await client.ConnectAsync(new Uri(serverUrl), CancellationToken.None);

            Data = data;
            Connected?.Invoke();

            _ = Task.Run(ReceiveMessages);
            return true;
        }
        catch (WebSocketException ex)
        {
            ErrorThrew?.Invoke($"An error occurred while connecting to the WebSocket server: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">The message to send to the server.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SendAsync(string message)
    {
        if (client.State != WebSocketState.Open)
        {
            ErrorThrew?.Invoke("WebSocket is not connected.");
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task CloseAsync()
    {
        if (client.State == WebSocketState.Open)
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
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
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            MessageReceived?.Invoke(result, message);
        }
    }

    private static async Task<bool> ValidateServerConnectionAsync(ConnectionData data)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(ServerTimeoutSeconds) };
        HttpResponseMessage response;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(ServerTimeoutSeconds));
            var serverUri = new Uri(data.GetServerHttpUrl());
            response = await httpClient.GetAsync(serverUri, cts.Token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            ErrorThrew?.Invoke("The request timed out.");
            return false;
        }
        catch (Exception ex)
        {
            ErrorThrew?.Invoke($"An error occurred while sending HTTP request: {ex.Message}");
            return false;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            string errorMessage = (int)response.StatusCode switch
            {
                >= 400 and < 500 => await response.Content.ReadAsStringAsync(),
                _ => $"Unexpected response from server: {response.StatusCode}",
            };

            ErrorThrew?.Invoke($"Server error: {errorMessage}");
            return false;
        }

        return true;
    }
}
