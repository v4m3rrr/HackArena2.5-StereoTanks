namespace GameLogic;

/// <summary>
/// Handles laser duration, damage application, and mine destruction.
/// </summary>
/// <param name="grid">The grid containing the lasers.</param>
/// <param name="damageSystem">The damage system for applying damage.</param>
/// <param name="scoreSystem">The score system for awarding points.</param>
/// <param name="stunSystem">The stun system for applying stuns.</param>
/// <param name="mineSystem">The mine system for handling mines.</param>
internal sealed class LaserSystem(
    Grid grid,
    DamageSystem damageSystem,
    ScoreSystem scoreSystem,
    StunSystem stunSystem,
    MineSystem mineSystem)
{
    private const int LaserDamage = 80;

    /// <summary>
    /// Attempts to use the laser ability and spawn laser beams.
    /// </summary>
    /// <param name="ability">The laser ability to activate.</param>
    /// <returns>
    /// The spawned lasers if successful; otherwise, <see langword="null"/>.
    /// </returns>
    public List<Laser>? TryUseLaser(LaserAbility ability)
    {
        if (!ability.CanUse)
        {
            return null;
        }

        var turret = ability.Turret;
        var turretDirection = turret.Direction;
        var (nx, ny) = DirectionUtils.Normal(turretDirection);

        var tiles = new List<(int X, int Y)>();

        var currX = ability.Turret.Tank.X + nx;
        var currY = ability.Turret.Tank.Y + ny;

        while (currX >= 0 && currX < grid.Dim && currY >= 0 && currY < grid.Dim)
        {
            if (grid.WallGrid[currX, currY] is not null)
            {
                break;
            }

            tiles.Add((currX, currY));

            currX += nx;
            currY += ny;
        }

        var lasers = new List<Laser>();
        var orientation = DirectionUtils.ToOrientation(turretDirection);

        foreach (var (x, y) in tiles)
        {
            var laser = new Laser(x, y, orientation, LaserDamage, turret.Tank.Owner);
            lasers.Add(laser);
        }

        grid.Lasers.AddRange(lasers);
        stunSystem.ApplyStuns(ability.Turret.Tank, lasers);
        ability.Use();

        return lasers;
    }

    /// <summary>
    /// Updates lasers, applies damage, and removes expired ones.
    /// </summary>
    public void Update()
    {
        foreach (Laser laser in grid.Lasers.ToList())
        {
            laser.DecreaseRemainingTicks();

            if (laser.RemainingTicks <= 0)
            {
                _ = grid.Lasers.Remove(laser);
                continue;
            }

            var tanks = grid.Tanks.Where(t => t.X == laser.X && t.Y == laser.Y);
            foreach (Tank tank in tanks)
            {
                var dealt = damageSystem.ApplyDamage(tank, laser.Damage!.Value, laser.Shooter);

                if (laser.Shooter is { } shooter)
                {
#if STEREO
                    if (!shooter.Team.Equals(tank.Owner.Team))
#endif
                    {
                        scoreSystem.AwardScore(shooter, dealt);
                    }
                }
            }

            mineSystem.TryExplodeAt(laser.X, laser.Y);
        }
    }
}
