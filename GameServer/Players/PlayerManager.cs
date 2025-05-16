using GameLogic;
using Serilog;

namespace GameServer;

/// <summary>
/// Manages players joining and leaving the game.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger instance.</param>
internal sealed class PlayerManager(GameInstance game, ILogger logger)
{
    private static readonly Random Random = new();

    private static readonly uint[] Colors =
    [
#if !STEREO
        0xFFFF48C5,
#endif
        0xFF1F2AFF,
#if !STEREO
        0xFF2ACBEB,
#endif
        0xFF36DE27,
    ];

#if STEREO
    /// <summary>
    /// Creates a new player and adds them to the team.
    /// </summary>
    /// <param name="connectionData">The player connection data.</param>
    /// <param name="team">The resulting team instance.</param>
    /// <returns>The player instance.</returns>
    public Player CreatePlayer(ConnectionData.Player connectionData, out Team team)
#else
    /// <summary>
    /// Creates a new player.
    /// </summary>
    /// <param name="connectionData">The player connection data.</param>
    /// <returns>The player instance.</returns>
    public Player CreatePlayer(ConnectionData.Player connectionData)
#endif
    {
        logger.Verbose("Creating player for connection {connection}", connectionData);

        string id;
        do
        {
            id = Guid.NewGuid().ToString();
        } while (game.Players.Any(p => p.Instance.Id == id));

#if STEREO
        var playerIdentifier = $"{connectionData.TeamName}/{connectionData.TankType}";
        var teamName = connectionData.TeamName;
        var color = this.GetTeamColor(teamName);
#else
        var playerIdentifier = connectionData.Nickname;
        var color = this.GetPlayerColor();
#endif

#if STEREO
        var foundTeam = game.Teams.FirstOrDefault(t => t.Name == teamName);
        team = foundTeam ?? new Team(teamName, color);
        if (foundTeam is null)
        {
            logger.Verbose("Created new team: {teamName}", teamName);
        }
#endif

        logger.Verbose("Assigned ID {id} and color {color} to player {playerIdentifier}", id, color, playerIdentifier);

        var instance = new Player(id)
        {
#if STEREO
            Team = team,
#else
            Color = color,
            Nickname = connectionData.Nickname,
#endif
        };

#if STEREO
        team.AddPlayer(instance);
#endif

        SpawnSystem spawner = game.Systems.Spawn;
        if (game.GameManager.Status is GameStatus.InLobby)
        {
#if STEREO
            instance.Tank = SpawnSystem.GenerateDeclaredTank(instance, connectionData.TankType);
#else
            instance.Tank = spawner.GenerateTank(instance);
#endif
        }
        else
        {
#if STEREO
            instance.Tank = spawner.GenerateTank(instance, connectionData.TankType);
#else
            instance.Tank = spawner.GenerateTank(instance);
#endif
        }

        return instance;
    }

    /// <summary>
    /// Removes a player from the game grid.
    /// </summary>
    /// <param name="player">The player connection.</param>
    public void RemovePlayer(PlayerConnection player)
    {
        logger.Verbose("Despawning tank for player {identifier}", player.Identifier);
        _ = game.Systems.Despawn.RemoveTank(player.Instance);

#if STEREO
        player.Instance.Team.RemovePlayer(player.Instance);

        if (!game.Teams.Any(t => t.Equals(player.Team)))
        {
            game.Systems.Despawn.RemoveTeam(player.Instance.Team);
        }
#endif
    }

#if STEREO
    private uint GetTeamColor(string teamName)
    {
        var existingTeam = game.Teams.FirstOrDefault(t => t.Name == teamName);
        if (existingTeam is not null)
        {
            return existingTeam.Color;
        }

        foreach (var color in Colors)
        {
            if (!game.Teams.Any(t => t.Color == color))
            {
                return color;
            }
        }

        return (uint)(0xFF << 24 | Random.Next(0xFFFFFF));
    }
#else
    private uint GetPlayerColor()
    {
        foreach (var color in Colors)
        {
            if (!game.Players.Any(p => p.Instance.Color == color))
            {
                return color;
            }
        }

        return (uint)(0xFF << 24 | Random.Next(0xFFFFFF));
    }
#endif
}
