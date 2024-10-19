using System.Net;
using System.Net.WebSockets;
using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents a connection to a spectator.
/// </summary>
/// <param name="Context">The HTTP listener context.</param>
/// <param name="Socket">The WebSocket.</param>
/// <param name="Data">The connection data of the spectator.</param>
/// <param name="Log">The logger.</param>
internal record class SpectatorConnection(
    HttpListenerContext Context,
    WebSocket Socket,
    ConnectionData Data,
    Logger Log)
    : Connection(Context, Socket, Data, Log)
{
    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{base.ToString()}, Type=Spectator";
    }
}
