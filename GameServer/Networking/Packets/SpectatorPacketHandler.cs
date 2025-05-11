using GameLogic.Networking;
using Serilog;

namespace GameServer;

#pragma warning disable CS9113

/// <summary>
/// A packet subhandler for spectator connections.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger.</param>
internal class SpectatorPacketHandler(GameInstance game, ILogger logger)
    : IPacketSubhandler
{
    /// <inheritdoc/>
    public bool CanHandle(Connection connection, Packet packet)
    {
        return connection is SpectatorConnection;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Always returns <see langword="false"/> because spectator-specific packets
    /// are not currently supported.
    /// </remarks>
    public Task<bool> HandleAsync(Connection connection, Packet packet)
    {
        return Task.FromResult(false);
    }
}
