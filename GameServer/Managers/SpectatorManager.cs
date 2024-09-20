using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace GameServer;

/// <summary>
/// Represents the spectator manager.
/// </summary>
internal class SpectatorManager
{
    /// <summary>
    /// Gets the spectators.
    /// </summary>
    /// <remarks>
    /// The value is not used,
    /// We use a ConcurrentDictionary with a dummy value instead.
    /// The value is a byte, because it is the smallest value type.
    /// </remarks>
    public ConcurrentDictionary<WebSocket, byte> Spectators { get; private set; } = [];

    /// <summary>
    /// Adds a spectator.
    /// </summary>
    /// <param name="socket">The socket of the spectator.</param>
    public void AddSpectator(WebSocket socket)
    {
        this.Spectators[socket] = 0;
    }

    /// <summary>
    /// Removes a spectator.
    /// </summary>
    /// <param name="socket">The socket of the spectator.</param>
    public void RemoveSpectator(WebSocket socket)
    {
        _ = this.Spectators.Remove(socket, out _);
    }

    /// <summary>
    /// Determines whether the specified socket is a spectator.
    /// </summary>
    /// <param name="socket">The socket of the spectator.</param>
    /// <returns>
    /// <see langword="true"/> if the specified socket is a spectator;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsSpectator(WebSocket socket)
    {
        return this.Spectators.ContainsKey(socket);
    }
}
