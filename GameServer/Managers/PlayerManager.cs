using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents the player manager.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="log">The logger.</param>
internal class PlayerManager(GameInstance game, Logger log)
{
    private static readonly Random Random = new();

    private static readonly uint[] Colors =
    [
        /* ABGR */
        0xFFFF48C5,
        0xFF1F2AFF,
        0xFF2ACBEB,
        0xFF36DE27,
    ];

    /// <summary>
    /// Adds a player.
    /// </summary>
    /// <param name="connectionData">The connection data of the player.</param>
    /// <returns>The player instance.</returns>
    public GameLogic.Player CreatePlayer(ConnectionData.Player connectionData)
    {
        log.Verbose("Creating player for ({connection})", connectionData);

        string id;
        do
        {
            id = Guid.NewGuid().ToString();
        } while (game.Players.Any(p => p.Instance.Id == id));

        var nickname = connectionData.Nickname;
        log.Verbose("Player ID: {id} (nickname)", id, nickname);

        var color = this.GetPlayerColor();
        log.Verbose("Player color: {color} (nickname)", color, nickname);

        log.Verbose("Creating player instance for ({nickname})", nickname);
        var instance = new GameLogic.Player(id, connectionData.Nickname, color);
        log.Verbose("Player instance created for ({nickname})", nickname);

        log.Verbose("Creating player tank ({nickname})", nickname);
        _ = game.Grid.GenerateTank(instance);
        log.Verbose("Player tank created ({nickname})", nickname);

        log.Verbose("Player created for ({connection})", connectionData);

        return instance;
    }

    /// <summary>
    /// Removes a player from the grid.
    /// </summary>
    /// <param name="player">The player connection.</param>
    public void RemovePlayer(PlayerConnection player)
    {
        log.Verbose("Removing player tank ({nickname})", player.Instance.Nickname);
        _ = game.Grid.RemoveTank(player.Instance);
        log.Verbose("Player tank removed ({nickname})", player.Instance.Nickname);
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
