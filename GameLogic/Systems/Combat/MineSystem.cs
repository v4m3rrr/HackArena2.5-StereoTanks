namespace GameLogic;

/// <summary>
/// Handles mine ticking, explosions, and cleanup.
/// </summary>
/// <param name="grid">The grid containing the mines.</param>
/// <param name="damageSystem">The damage system for applying damage.</param>
/// <param name="scoreSystem">The score system for awarding points.</param>
/// <param name="stunSystem">The stun system for applying stuns.</param>
internal sealed class MineSystem(
    Grid grid,
    DamageSystem damageSystem,
    ScoreSystem scoreSystem,
    StunSystem stunSystem)
{
    private const int MineDamage = 50;

    /// <summary>
    /// Attempts to drop a mine from the specified ability.
    /// </summary>
    /// <param name="ability">The mine ability component.</param>
    /// <returns>
    /// The dropped mine if successful; otherwise, <see langword="null"/>.
    /// </returns>
    public Mine? TryDropMine(MineAbility ability)
    {
        if (!ability.CanUse)
        {
            return null;
        }

        var tank = ability.Tank;
        var (dx, dy) = DirectionUtils.Normal(tank.Direction);
        var mineX = tank.X - dx;
        var mineY = tank.Y - dy;

        var mine = new Mine(
            mineX,
            mineY,
            MineDamage,
            ability.Tank.Owner);

        grid.Mines.Add(mine);
        ability.Use();

        return mine;
    }

    /// <summary>
    /// Attempts to trigger explosions for all mines located at the specified grid coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the grid cell to check for mines.</param>
    /// <param name="y">The y-coordinate of the grid cell to check for mines.</param>
    public void TryExplodeAt(int x, int y)
    {
        foreach (Mine mine in grid.Mines)
        {
            if (mine.X != x || mine.Y != y || mine.IsExploded)
            {
                continue;
            }

            Tank? tank = grid.Tanks.FirstOrDefault(t => t.X == mine.X && t.Y == mine.Y);
            this.HandleMineExplosion(mine, tank);
        }
    }

    /// <summary>
    /// Updates mine timers and triggers explosions when needed.
    /// </summary>
    public void Update()
    {
        foreach (Mine mine in grid.Mines.ToList())
        {
            if (!grid.IsCellWithinBounds(mine.X, mine.Y)
                || grid.WallGrid[mine.X, mine.Y] is not null)
            {
                _ = grid.Mines.Remove(mine);
                continue;
            }

            if (mine.IsFullyExploded)
            {
                _ = grid.Mines.Remove(mine);
                continue;
            }

            if (!mine.IsExploded)
            {
                Tank? tank = grid.Tanks.FirstOrDefault(t => t.X == mine.X && t.Y == mine.Y);
                if (tank is not null)
                {
                    this.HandleMineExplosion(mine, tank);
                }
            }
            else
            {
                mine.DecreaseExplosionTicks();
            }
        }

        // Remove duplicate mines.
        // Reverse the list to remove the last mine in case of duplicates.
        var mines = grid.Mines.ToList();
        var seen = new HashSet<(int, int)>();

        foreach (Mine mine in mines)
        {
            var key = (mine.X, mine.Y);
            _ = seen.Contains(key)
                ? grid.Mines.Remove(mine)
                : seen.Add(key);
        }
    }

    private void HandleMineExplosion(Mine mine, Tank? tank)
    {
        mine.Explode();

        if (tank is not null)
        {
            bool suicide = mine.LayerId == tank.Owner.Id;

            int dealt = damageSystem.ApplyDamage(
            tank,
            mine.Damage!.Value,
            suicide ? null : mine.Layer);

            if (mine.Layer is { } layer && !suicide)
            {
#if STEREO
                if (!layer.Team.Equals(tank.Owner.Team))
#endif
                {
                    scoreSystem.AwardScore(mine.Layer, dealt);
                }
            }

            stunSystem.ApplyStun(tank, mine);
        }
    }
}
