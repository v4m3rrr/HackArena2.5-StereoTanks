using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents a subhandler capable of handling specific packets
/// based on the connection and packet type.
/// </summary>
internal interface IPacketSubhandler
{
    /// <summary>
    /// Determines whether the given packet can be handled by this subhandler.
    /// </summary>
    /// <param name="connection">The connection from which the packet originated.</param>
    /// <param name="packet">The packet to be evaluated.</param>
    /// <returns>
    /// <see langword="true"/> if this subhandler can handle the packet;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool CanHandle(Connection connection, Packet packet);

    /// <summary>
    /// Handles the specified packet asynchronously.
    /// </summary>
    /// <param name="connection">The connection from which the packet originated.</param>
    /// <param name="packet">The packet to handle.</param>
    /// <returns>
    /// <see langword="true"/> if the packet was handled successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> HandleAsync(Connection connection, Packet packet);
}
