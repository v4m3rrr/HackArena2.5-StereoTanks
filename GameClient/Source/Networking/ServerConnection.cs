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
internal class ServerConnection
{
    private ClientWebSocket client = new();

    /// <summary>
    /// Occurs when a message is received from the server.
    /// </summary>
    public event Action<WebSocketReceiveResult, string>? MessageReceived;

    /// <summary>
    /// Occurs when the connection to the server is establishing.
    /// </summary>
    public event Action<string>? Connecting;

    /// <summary>
    /// Occurs when the connection to the server is established.
    /// </summary>
    public event Action? Connected;

    /// <summary>
    /// Occurs when an error occurs.
    /// </summary>
    public event Action<string>? ErrorThrew;

    /// <summary>
    /// Gets or sets the buffer size for the WebSocket messages.
    /// </summary>
    public int BufferSize { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the timeout for the server connection in seconds.
    /// </summary>
    public int ServerTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets a value indicating whether the WebSocket is connected.
    /// </summary>
    public bool IsConnected => this.client.State == WebSocketState.Open;

    /// <summary>
    /// Gets the connection data.
    /// </summary>
    public ConnectionData ConnectionData { get; private set; }

    /// <summary>
    /// Connects to the specified server.
    /// </summary>
    /// <param name="data">The connection data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<bool> ConnectAsync(ConnectionData data)
    {
        string serverUrl = data.GetServerWsUrl();

        this.Connecting?.Invoke(serverUrl);

        if (!await this.ValidateServerConnectionAsync(data))
        {
            return false;
        }

        try
        {
            this.client = new ClientWebSocket();
            await this.client.ConnectAsync(new Uri(serverUrl), CancellationToken.None);

            this.ConnectionData = data;
            this.Connected?.Invoke();

            _ = Task.Run(this.ReceiveMessages);
            return true;
        }
        catch (WebSocketException ex)
        {
            this.ErrorThrew?.Invoke($"An error occurred while connecting to the WebSocket server: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">The message to send to the server.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendAsync(string message)
    {
        if (this.client.State != WebSocketState.Open)
        {
            this.ErrorThrew?.Invoke("WebSocket is not connected.");
            return;
        }

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        var messageSegment = new ArraySegment<byte>(messageBytes);

        try
        {
            await this.client.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            this.ErrorThrew?.Invoke($"An error occurred while sending the message: {ex.Message}");
        }
    }

    /// <summary>
    /// Closes the WebSocket connection.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CloseAsync()
    {
        if (this.client.State == WebSocketState.Open)
        {
            await this.client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    private async Task ReceiveMessages()
    {
        while (this.client.State == WebSocketState.Open)
        {
            byte[] buffer = new byte[this.BufferSize];
            WebSocketReceiveResult result;

            try
            {
                result = await this.client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                this.ErrorThrew?.Invoke($"A WebSocket error occurred: {ex.Message}");
                break;
            }

            if (result.CloseStatus.HasValue)
            {
                await this.client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            this.MessageReceived?.Invoke(result, message);
        }
    }

    private async Task<bool> ValidateServerConnectionAsync(ConnectionData data)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(this.ServerTimeoutSeconds) };
        HttpResponseMessage response;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(this.ServerTimeoutSeconds));
            var serverUri = new Uri(data.GetServerHttpUrl());
            response = await httpClient.GetAsync(serverUri, cts.Token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            this.ErrorThrew?.Invoke("The request timed out.");
            return false;
        }
        catch (Exception ex)
        {
            this.ErrorThrew?.Invoke($"An error occurred while sending HTTP request: {ex.Message}");
            return false;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            string errorMessage = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.TooManyRequests => await response.Content.ReadAsStringAsync(),
                _ => $"Unexpected response from server: {response.StatusCode}",
            };

            this.ErrorThrew?.Invoke($"Server error: {errorMessage}");
            return false;
        }

        return true;
    }
}
