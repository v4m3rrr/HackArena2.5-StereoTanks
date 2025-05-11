using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Represents the lobby manager.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger.</param>
internal class LobbyManager(GameInstance game, ILogger logger)
{
    /// <summary>
    /// Sends the lobby data to all players and spectators.
    /// </summary>
    /// <returns>A task representing the asynchronous operations.</returns>
    public async Task SendLobbyDataToAll()
    {
        logger.Verbose("Sending lobby data to all clients...");

        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendLobbyDataTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        logger.Verbose("Lobby data sent to all clients.");
    }

    /// <summary>
    /// Sends the lobby data to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the lobby data to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendLobbyDataTo(Connection connection)
    {
        logger.Verbose("Sending lobby data to ({connection}).", connection);

        var payload = game.PayloadHelper.GetLobbyDataPayload(connection, out var converters);
        var packet = new ResponsePacket(payload, logger, converters);
        await packet.SendAsync(connection);

        logger.Verbose("Lobby data sent to ({connection}).", connection);
    }

    /// <summary>
    /// Sends the game starting to all players and spectators.
    /// </summary>
    /// <returns>A tasks representing the asynchronous operations.</returns>
    public async Task SendGameStartingToAll()
    {
        logger.Information("Sending game starting to all clients...");

        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendGameStartingTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        logger.Information("Game starting sent to all clients.");
    }

    /// <summary>
    /// Sends the game start to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the game start to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendGameStartingTo(Connection connection)
    {
        logger.Verbose("Sending game starting to ({connection}).", connection);

        var payload = new EmptyPayload() { Type = PacketType.GameStarting };
        var packet = new ResponsePacket(payload, logger);
        await packet.SendAsync(connection);

        logger.Verbose("Game starting sent to ({connection}).", connection);
    }

    /// <summary>
    /// Sends the game started to all players and spectators.
    /// </summary>
    /// <returns>A tasks representing the asynchronous operations.</returns>
    public async Task SendGameStartedToAll()
    {
        logger.Information("Sending game started to all clients...");

        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendGameStartedTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        logger.Information("Game started sent to all clients.");
    }

    /// <summary>
    /// Sends the game start to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the game start to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendGameStartedTo(Connection connection)
    {
        logger.Verbose("Sending game started to ({connection}).", connection);

        var payload = new EmptyPayload() { Type = PacketType.GameStarted };
        var packet = new ResponsePacket(payload, logger);
        await packet.SendAsync(connection);

        logger.Verbose("Game started sent to ({connection}).", connection);
    }
}
