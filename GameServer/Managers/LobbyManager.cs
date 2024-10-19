using GameLogic.Networking;
using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents the lobby manager.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="log">The logger.</param>
internal class LobbyManager(GameInstance game, Logger log)
{
    /// <summary>
    /// Sends the lobby data to all players and spectators.
    /// </summary>
    /// <returns>A task representing the asynchronous operations.</returns>
    public async Task SendLobbyDataToAll()
    {
        log.Verbose("Sending lobby data to all clients...");

        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendLobbyDataTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        log.Verbose("Lobby data sent to all clients.");
    }

    /// <summary>
    /// Sends the lobby data to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the lobby data to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendLobbyDataTo(Connection connection)
    {
        log.Verbose("Sending lobby data to ({connection}).", connection);

        var payload = game.PayloadHelper.GetLobbyDataPayload(connection, out var converters);
        var packet = new ResponsePacket(payload, log, converters);
        await packet.SendAsync(connection);

        log.Verbose("Lobby data sent to ({connection}).", connection);
    }

    /// <summary>
    /// Sends the game starting to all players and spectators.
    /// </summary>
    /// <returns>A tasks representing the asynchronous operations.</returns>
    public async Task SendGameStartingToAll()
    {
        log.Information("Sending game starting to all clients...");

        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendGameStartingTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        log.Information("Game starting sent to all clients.");
    }

    /// <summary>
    /// Sends the game start to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the game start to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendGameStartingTo(Connection connection)
    {
        log.Verbose("Sending game starting to ({connection}).", connection);

        var payload = new EmptyPayload() { Type = PacketType.GameStarting };
        var packet = new ResponsePacket(payload, log);
        await packet.SendAsync(connection);

        log.Verbose("Game starting sent to ({connection}).", connection);
    }

    /// <summary>
    /// Sends the game started to all players and spectators.
    /// </summary>
    /// <returns>A tasks representing the asynchronous operations.</returns>
    public async Task SendGameStartedToAll()
    {
        log.Information("Sending game started to all clients...");

        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendGameStartedTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        log.Information("Game started sent to all clients.");
    }

    /// <summary>
    /// Sends the game start to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the game start to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendGameStartedTo(Connection connection)
    {
        log.Verbose("Sending game started to ({connection}).", connection);

        var payload = new EmptyPayload() { Type = PacketType.GameStarted };
        var packet = new ResponsePacket(payload, log);
        await packet.SendAsync(connection);

        log.Verbose("Game started sent to ({connection}).", connection);
    }
}
