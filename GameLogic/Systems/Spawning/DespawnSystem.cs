namespace GameLogic;

#pragma warning disable CS9113

/// <summary>
/// Handles removal of tanks and related cleanup from the grid.
/// </summary>
/// <param name="grid">The grid instance.</param>
/// <param name="healSystem">The heal system for resetting tank health.</param>
/// <param name="scoreSystem">The score system for resetting player scores.</param>
/// <param name="zoneSystem">The zone system for resetting zone states.</param>
internal sealed class DespawnSystem(Grid grid, HealSystem healSystem, ScoreSystem scoreSystem, ZoneSystem zoneSystem)
{
#if !STEREO

    /// <summary>
    /// Gets the item drop system.
    /// </summary>
    public required ItemDropSystem ItemDropSystem { private get; init; }

#endif

    /// <summary>
    /// Removes the tank of a given player and cleans up related game objects.
    /// </summary>
    /// <param name="owner">The player whose tank should be removed.</param>
    /// <returns>The removed tank if found; otherwise, <see langword="null"/>.</returns>
    public Tank? RemoveTank(Player owner)
    {
        var tank = grid.Tanks.FirstOrDefault(t => t.Owner.Equals(owner));

        if (tank is null)
        {
            return null;
        }

#if !STEREO
        this.ItemDropSystem.TryDropItem(tank);
#endif

        healSystem.ClearFractionalBuffer(tank);

#if !STEREO
        scoreSystem.OnPlayerRemoved(owner);
        zoneSystem.OnPlayerRemoved(owner);
#endif

        _ = grid.Tanks.Remove(tank);
        _ = grid.Bullets.RemoveAll(b => b.Shooter?.Equals(owner) ?? false);
        _ = grid.Lasers.RemoveAll(l => l.Shooter?.Equals(owner) ?? false);
        _ = grid.Mines.RemoveAll(m => m.Layer?.Equals(owner) ?? false);

        return tank;
    }

#if STEREO

    /// <summary>
    /// Removes the team from the game grid and cleans up related game objects.
    /// </summary>
    /// <param name="team">The team to remove from the game grid.</param>
    public void RemoveTeam(Team team)
    {
        zoneSystem.OnTeamRemoved(team);
    }

#endif
}
