using System.Collections.Concurrent;
using System.Net.WebSockets;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the player manager.
/// </summary>
/// <param name="game">The game instance.</param>
internal class PlayerManager(GameInstance game)
{
    private static readonly Random Random = new();

    private static readonly uint[] Colors =
    [
        /* ABGR */
        0xFFFFA600,
        0xFFFF5AF9,
        0xFF1A9BF9,
        0xFF3FD47A,
    ];

    /// <summary>
    /// Adds a player.
    /// </summary>
    /// <param name="socket">The socket of the player.</param>
    /// <param name="connectionData">The connection data of the player.</param>
    /// <returns>The player instance.</returns>
    public GameLogic.Player CreatePlayer(WebSocket socket, ConnectionData.Player connectionData)
    {
        string id;
        do
        {
            id = Guid.NewGuid().ToString();
        } while (game.Players.Any(p => p.Instance.Id == id));

        var color = this.GetPlayerColor();

        var instance = new GameLogic.Player(id, connectionData.Nickname, color);
        _ = game.Grid.GenerateTank(instance);

        return instance;
    }

    /// <summary>
    /// Removes a player from the grid.
    /// </summary>
    /// <param name="player">The player connection.</param>
    public void RemovePlayer(PlayerConnection player)
    {
        _ = game.Grid.RemoveTank(player.Instance);
    }

    private uint GetPlayerColor()
    {
        foreach (uint color in Colors)
        {
            if (!game.Players.Any(p => p.Instance.Color == color))
            {
                return color;
            }
        }

        return (uint)((0xFF << 24) | Random.Next(0xFFFFFF));
    }
}
