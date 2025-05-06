using GameLogic;
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
    /// Adds a player.
    /// </summary>
    /// <param name="connectionData">The connection data of the player.</param>
    /// <param name="team">The team of the player.</param>
    /// <returns>The player instance.</returns>
    public Player CreatePlayer(ConnectionData.Player connectionData, out Team team)
#else
    /// <summary>
    /// Adds a player.
    /// </summary>
    /// <param name="connectionData">The connection data of the player.</param>
    /// <returns>The player instance.</returns>
    public Player CreatePlayer(ConnectionData.Player connectionData)
#endif
    {
        log.Verbose("Creating player for ({connection})", connectionData);

        string id;
        do
        {
            id = Guid.NewGuid().ToString();
        } while (game.Players.Any(p => p.Instance.Id == id));

#if STEREO
        var playerIdentifier = $"{connectionData.TeamName}/{connectionData.TankType}";
#else
        var playerIdentifier = connectionData.Nickname;
#endif

        log.Verbose("Player ID: {id} ({playerIdentifier})", id, playerIdentifier);

#if STEREO
        var teamName = connectionData.TeamName;
        var color = this.GetTeamColor(teamName);
#else
        var color = this.GetPlayerColor();
#endif
        log.Verbose("Player color: {color} ({playerIdentifier})", color, playerIdentifier);

#if STEREO
        log.Verbose("Trying to get team ({teamName})", teamName);
        var foundTeam = game.Teams.FirstOrDefault(t => t.Name == teamName);
        if (foundTeam is null)
        {
            log.Verbose("Team not found ({teamName}), creating a new one", teamName);
            team = new Team(teamName, color);
            log.Verbose("New team created ({teamName})", connectionData.TeamName);
        }
        else
        {
            log.Verbose("Team found ({teamName})", connectionData.TeamName);
            team = foundTeam;
        }
#endif

        log.Verbose("Creating player instance for ({playerIdentifier})", playerIdentifier);
        var instance = new Player(id)
        {
#if STEREO
            Team = team,
#else
            Color = color,
            Nickname = connectionData.Nickname,
#endif
        };
        log.Verbose("Player instance created for ({playerIdentifier})", playerIdentifier);

#if STEREO
        log.Verbose("Adding player to team ({teamName})", team.Name);
        team.AddPlayer(instance);
        log.Verbose("Player added to team ({teamName})", team.Name);
#endif

        log.Verbose("Creating player tank ({playerIdentifier})", playerIdentifier);

#if STEREO
        _ = game.GameManager.Status is not GameStatus.Running
            ? Grid.GenerateDeclaredTankStub(instance, connectionData.TankType)
            : game.Grid.GenerateTank(instance, connectionData.TankType);
#else
        _ = game.Grid.GenerateTank(instance);
#endif
        log.Verbose("Player tank created ({playerIdentifier})", playerIdentifier);

        log.Verbose("Player created for ({connection})", connectionData);

        return instance;
    }

    /// <summary>
    /// Removes a player from the grid.
    /// </summary>
    /// <param name="player">The player connection.</param>
    public void RemovePlayer(PlayerConnection player)
    {
        log.Verbose("Removing player tank ({identifier})", player.Identifier);
        _ = game.Grid.RemoveTank(player.Instance);
        log.Verbose("Player tank removed ({identifier})", player.Identifier);
    }

#if STEREO

    private uint GetTeamColor(string teamName)
    {
        var team = game.Teams.FirstOrDefault(t => t.Name == teamName);
        if (team is not null)
        {
            return team.Color;
        }

        foreach (uint color in Colors)
        {
            if (!game.Teams.Any(t => t.Color == color))
            {
                return color;
            }
        }

        return (uint)((0xFF << 24) | Random.Next(0xFFFFFF));
    }

#else

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

#endif
}
