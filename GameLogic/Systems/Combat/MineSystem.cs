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
#if STEREO
    private const int MineDamage = 35;
#else
    private const int MineDamage = 50;
#endif

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

            this.HandleMineExplosion(mine);
        }
    }

    /// <summary>
    /// Updates mine timers and triggers explosions when needed.
    /// </summary>
    public void Update()
    {
#if STEREO && SERVER
        foreach (Mine mine in grid.Mines.ToList())
        {
            if (mine.ShouldExplodeNextTick)
            {
                mine.ShouldExplodeNextTick = false;
                this.HandleMineExplosion(mine);
            }
        }
#endif

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

            if (!mine.IsExploded && grid.Tanks.Any(t => t.X == mine.X && t.Y == mine.Y))
            {
                this.HandleMineExplosion(mine);
            }
            else if (mine.ExplosionRemainingTicks is not null)
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

    private void HandleMineExplosion(Mine mine)
    {
        void TryDamageTankAt(int x, int y, float multiplier)
        {
            Tank? tank = grid.Tanks.FirstOrDefault(t => t.X == x && t.Y == y);

            if (tank is null)
            {
                return;
            }

            var suicide = mine.LayerId == tank.Owner.Id;

#if STEREO
            suicide &= mine.Layer!.Team.Equals(tank.Owner.Team);
#endif

            int dealt = damageSystem.ApplyDamage(
                tank,
                (int)((mine.Damage!.Value * multiplier) + 0.5f),
                suicide ? null : mine.Layer);

            if (mine.Layer is { } layer && !suicide)
            {
                scoreSystem.AwardScore(layer, dealt);
            }

            var stunEffect = (mine as IStunEffect).StunBlockEffect;
            var stunTicks = (mine as IStunEffect).StunTicks;
            stunSystem.ApplyStun(tank, stunEffect, (int)((stunTicks * multiplier) + 0.5f));
        }

        TryDamageTankAt(mine.X, mine.Y, multiplier: 1f);
        mine.Explode();

#if STEREO && SERVER
        static bool IsDiagonal(int dx, int dy) => dx != 0 && dy != 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                int x = mine.X + dx;
                int y = mine.Y + dy;
                float multiplier = IsDiagonal(dx, dy) ? 0.45f : 0.7f;

                TryDamageTankAt(x, y, multiplier);

                if (grid.Mines.FirstOrDefault(m => m.X == x && m.Y == y && !m.IsExploded) is Mine otherMine)
                {
                    otherMine.ShouldExplodeNextTick = true;
                }
            }
        }
#endif
    }
}
