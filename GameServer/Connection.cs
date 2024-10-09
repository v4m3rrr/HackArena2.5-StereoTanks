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

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"IP={this.Ip}, SocketState={this.Socket.State}";
    }
}
