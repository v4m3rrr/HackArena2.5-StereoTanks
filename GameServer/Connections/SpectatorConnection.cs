using System.Net;
using System.Net.WebSockets;

namespace GameServer;

/// <summary>
/// Represents a connection to a spectator.
/// </summary>
/// <param name="Context">The HTTP listener context.</param>
/// <param name="Socket">The WebSocket.</param>
/// <param name="Data">The connection data of the spectator.</param>
internal record class SpectatorConnection(HttpListenerContext Context, WebSocket Socket, ConnectionData Data)
    : Connection(Context, Socket, Data)
{
    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{base.ToString()}, Type=Spectator";
    }
}
