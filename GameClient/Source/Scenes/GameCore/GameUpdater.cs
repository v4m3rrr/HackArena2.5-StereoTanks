using GameLogic;
using GameLogic.Networking;

namespace GameClient.Scenes.GameCore;

#if STEREO
#pragma warning disable IDE0079
#pragma warning disable SA1101
/// <summary>
/// Represents a game updater.
/// </summary>
/// <param name="components">The game screen components.</param>
/// <param name="teams">The list of teams.</param>
internal class GameUpdater(GameComponents components, List<Team> teams)
#else
/// <summary>
/// Represents a game updater.
/// </summary>
/// <param name="components">The game screen components.</param>
/// <param name="players">The list of players.</param>
internal class GameUpdater(GameComponents components, Dictionary<string, Player> players)
#endif
{
    private readonly object playerUpdateLock = new();

#if STEREO
#pragma warning disable IDE1006, SA1300
    private Dictionary<string, Player> players
        => teams.SelectMany(t => t.Players).ToDictionary(p => p.Id, p => p);
#pragma warning restore IDE1006, SA1300
#endif

    /// <summary>
    /// Enables the grid component.
    /// </summary>
    public void EnableGridComponent()
    {
        components.Grid.IsEnabled = true;
    }

    /// <summary>
    /// Disables the grid component.
    /// </summary>
    public void DisableGridComponent()
    {
        components.Grid.IsEnabled = false;
    }

    /// <summary>
    /// Updates the grid logic from the game state payload.
    /// </summary>
    /// <param name="payload">The game state payload.</param>
    public void UpdateGrid(GameStatePayload payload)
    {
        try
        {
            GameStateApplier.ApplyToGrid(components.Grid.Logic, payload);
            components.Grid.Sync();
        }
        catch (Exception ex)
        {
            DebugConsole.ThrowError("An error occurred while updating the grid.");
            DebugConsole.ThrowError(ex, withTraceback: true);
        }
    }

#if STEREO

    /// <summary>
    /// Updates the list of teams.
    /// </summary>
    /// <param name="updatedTeams">The list of teams received from the server.</param>
    public void UpdateTeams(List<Team> updatedTeams)
    {
        lock (this.playerUpdateLock)
        {
            foreach (Team updatedTeam in updatedTeams)
            {
                if (teams.All(t => !t.Equals(updatedTeam)))
                {
                    teams.Add(updatedTeam);
                }
                else
                {
                    var existingTeam = teams.First(t => t.Equals(updatedTeam));
                    existingTeam.UpdateFrom(updatedTeam);
                }
            }

            teams
                .Where(t => !updatedTeams.Contains(t))
                .ToList()
                .ForEach(t => teams.Remove(t));

            foreach (Team team in teams)
            {
                foreach (Player player in team.Players)
                {
                    player.Team = team;
                }
            }
        }
    }

#else

    /// <summary>
    /// Updates the list of players.
    /// </summary>
    /// <param name="updatedPlayers">
    /// The list of players received from the server.
    /// </param>
    public void UpdatePlayers(List<Player> updatedPlayers)
    {
        lock (this.playerUpdateLock)
        {
            foreach (Player updatedPlayer in updatedPlayers)
            {
                if (players.TryGetValue(updatedPlayer.Id, out var existingPlayer))
                {
                    existingPlayer.UpdateFrom(updatedPlayer);
                }
                else
                {
                    players[updatedPlayer.Id] = updatedPlayer;
                }
            }

            players
                .Where(x => !updatedPlayers.Contains(x.Value))
                .ToList()
                .ForEach(x => players.Remove(x.Key));
        }
    }

#endif

#if HACKATHON

    /// <summary>
    /// Updates the match name.
    /// </summary>
    /// <param name="matchName">The match name to set.</param>
    public void UpdateMatchName(string? matchName)
    {
        components.MatchName.IsEnabled = matchName is not null;
        components.MatchName.Value = matchName ?? "-";
    }

#endif

#if STEREO

    /// <summary>
    /// Refreshes the team bar panels.
    /// </summary>
    /// <param name="teams">The collection of teams.</param>
    /// <remarks>
    /// <para>
    /// This method adjusts the team bar panels
    /// to the current list of players.
    /// </para>
    /// <para>
    /// Should be called after <see cref="UpdateTeams"/>.
    /// </para>
    /// </remarks>
    public void RefreshTeamBarPanels(IEnumerable<Team> teams)
    {
        GameClientCore.InvokeOnMainThread(() =>
        {
            lock (this.playerUpdateLock)
            {
                var team = teams.FirstOrDefault(x => x.Players.Any(p => p.Id == Game.PlayerId));
                components.TeamBarPanels
                    .Zip(teams, (panel, team) => (panel, team))
                    .Take(2)
                    .ToList()
                    .ForEach(x => x.panel.Refresh(x.team, team?.Name));
            }
        });
    }

#else

    /// <summary>
    /// Refreshes the player bar panels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method adjusts the player bar panels
    /// to the current list of players.
    /// </para>
    /// <para>
    /// Should be called after <see cref="UpdatePlayers"/>.
    /// </para>
    /// </remarks>
    public void RefreshPlayerBarPanels()
    {
        GameClientCore.InvokeOnMainThread(() =>
        {
            lock (this.playerUpdateLock)
            {
                components.PlayerIdentityBarPanel.Refresh(players);
                components.PlayerStatsBarPanel.Refresh(players, Game.PlayerId);
            }
        });
    }

#endif

    /// <summary>
    /// Updates the timer.
    /// </summary>
    /// <param name="tick">The current tick of the game.</param>
    public void UpdateTimer(int tick)
    {
        var time = Game.ServerBroadcastInterval * tick;
        components.Timer.Time = time;
    }
}
