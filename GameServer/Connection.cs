using System.Net;
using System.Net.WebSockets;

namespace GameServer;

/// <summary>
/// Represents a connection.
/// </summary>
/// <param name="Context">The HTTP listener context.</param>
/// <param name="Socket">The WebSocket.</param>
/// <param name="Data">The connection data.</param>
internal abstract record class Connection(HttpListenerContext Context, WebSocket Socket, ConnectionData Data)
{
    private readonly object lastPingSentTimeLock = new();
    private readonly object hasSentPongLock = new();

    private DateTime lastPingSendTime;
    private bool hasSentPong;

    /// <summary>
    /// Gets the SemaphoreSlim for sending packets.
    /// </summary>
    public SemaphoreSlim SendPacketSemaphore { get; } = new(1, 1);

    /// <summary>
    /// Gets the ip of the client.
    /// </summary>
    public string Ip { get; } = Context.Request.RemoteEndPoint.Address.ToString();

    /// <summary>
    /// Gets or sets the time when the last ping was sent.
    /// </summary>
    public DateTime LastPingSentTime
    {
        get
        {
            lock (this.lastPingSentTimeLock)
            {
                return this.lastPingSendTime;
            }
        }

        set
        {
            lock (this.lastPingSentTimeLock)
            {
                this.lastPingSendTime = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a player has sent a pong.
    /// </summary>
    /// <remarks>
    /// This property is thread-safe.
    /// </remarks>
    public bool HasSentPong
    {
        get
        {
            lock (this.hasSentPongLock)
            {
                return this.hasSentPong;
            }
        }

        set
        {
            lock (this.hasSentPongLock)
            {
                this.hasSentPong = value;
            }
        }
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    /// <param name="status">The status of the close.</param>
    /// <param name="description">The description of the close.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CloseAsync(
        WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
        string description = "Closing",
        CancellationToken cancellationToken = default)
    {
        await this.SendPacketSemaphore.WaitAsync(cancellationToken);

        try
        {
            await this.Socket.CloseAsync(status, description, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Error while closing the connection:");
            Console.WriteLine("[^^^^^] Status: {0}", status);
            Console.WriteLine("[^^^^^] Description: {0}", description);
            Console.WriteLine("[^^^^^] Connection: {0}", this);
            Console.WriteLine("[^^^^^] Message: {0}", ex.Message);
        }
        finally
        {
            _ = this.SendPacketSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"IP={this.Ip}, SocketState={this.Socket.State}";
    }
}
