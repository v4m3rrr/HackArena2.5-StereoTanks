using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the lobby manager.
/// </summary>
/// <param name="game">The game instance.</param>
internal class LobbyManager(GameInstance game)
{
    /// <summary>
    /// Sends the lobby data to all players and spectators.
    /// </summary>
    /// <returns>A task representing the asynchronous operations.</returns>
    public async Task SendLobbyDataToAll()
    {
        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendLobbyDataTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends the lobby data to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the lobby data to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendLobbyDataTo(Connection connection)
    {
        var payload = game.PayloadHelper.GetLobbyDataPayload(connection, out var converters);
        var packet = new ResponsePacket(payload, converters);
        await packet.SendAsync(connection);
    }

    /// <summary>
    /// Sends the game starting to all players and spectators.
    /// </summary>
    /// <returns>A tasks representing the asynchronous operations.</returns>
    public async Task SendGameStartingToAll()
    {
        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendGameStartingTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends the game start to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the game start to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendGameStartingTo(Connection connection)
    {
        var payload = new EmptyPayload() { Type = PacketType.GameStarting };
        var packet = new ResponsePacket(payload);
        await packet.SendAsync(connection);
    }

    /// <summary>
    /// Sends the game started to all players and spectators.
    /// </summary>
    /// <returns>A tasks representing the asynchronous operations.</returns>
    public async Task SendGameStartedToAll()
    {
        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var task = this.SendGameStartedTo(connection);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends the game start to a connection.
    /// </summary>
    /// <param name="connection">The connection to send the game start to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendGameStartedTo(Connection connection)
    {
        var payload = new EmptyPayload() { Type = PacketType.GameStarted };
        var packet = new ResponsePacket(payload);
        await packet.SendAsync(connection);
    }
}
