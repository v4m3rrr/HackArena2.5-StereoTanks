namespace GameLogic;

/// <summary>
/// A helper class for detecting collisions.
/// </summary>
public static class CollisionDetector
{
    /// <summary>
    /// Checks if the tank collides with something.
    /// </summary>
    /// <param name="bullet">The bullet to check for collision.</param>
    /// <param name="grid">The grid with all collidable objects.</param>
    /// <param name="trajectories">\The trajectories of the bullets.</param>
    /// <returns>The collision that occurred, or <see langword="null"/> if no collision occurred.</returns>
    public static Collision? CheckBulletCollision(
        Bullet bullet,
        Grid grid,
        Dictionary<Bullet, List<(int X, int Y)>> trajectories)
    {
        if (!trajectories.TryGetValue(bullet, out var trajectory))
        {
            // Temporary solution for bullets created by downgraded double bullets,
            // which are not in the trajectories dictionary.
            return null;
        }

        for (int i = 0; i < trajectories.First().Value.Count; i++)
        {
            if (i >= trajectory.Count)
            {
                break;
            }

            var (x, y) = trajectory[i];

            if (x < 0 || x >= grid.Dim || y < 0 || y >= grid.Dim)
            {
                return new Collision(CollisionType.Border);
            }

            if (grid.WallGrid[x, y] is not null)
            {
                return new Collision(CollisionType.Wall);
            }

            if (grid.Lasers.FirstOrDefault(l => l.X == x && l.Y == y) is not null)
            {
                return new Collision(CollisionType.Laser);
            }

            foreach (var otherBullet in trajectories.Keys.Where(x => x != bullet))
            {
                if (trajectories.TryGetValue(otherBullet, out var bullet2Trajectory))
                {
                    bool arePerpendicular = DirectionUtils.ArePerpendicular(bullet.Direction, otherBullet.Direction);
                    bool perpendicularCollision = arePerpendicular && i < bullet2Trajectory.Count && bullet2Trajectory[i].X == x && bullet2Trajectory[i].Y == y;
                    bool parallelCollision = !arePerpendicular && bullet2Trajectory.Any(c => c.X == x && c.Y == y);

                    if (perpendicularCollision || parallelCollision)
                    {
                        return new BulletCollision(otherBullet);
                    }

                    /* REFACTOR !!!
                     *
                     * The code below fixes an edge case where bullets may pass through
                     * each other without colliding, even though they should. This occurs
                     * when they are on parallel trajectories and no tiles overlap, particularly
                     * with a speed of 2 when they are 4 tiles apart.
                     *
                     * To improve this, it would be beneficial to refactor the entire collision
                     * detection logic and consider implementing a more general approach to collision
                     * detection that could better handle various types of interactions in the game.
                     *
                     */

                    bool areDirectlyOpposite = Math.Abs(bullet.Direction - otherBullet.Direction) == 2;
                    if (areDirectlyOpposite)
                    {
                        var (bnx, bny) = DirectionUtils.Normal(bullet.Direction);
                        var (b2nx, b2ny) = DirectionUtils.Normal(otherBullet.Direction);

                        var bulletPos = (trajectory[0].X - bnx, trajectory[0].Y - bny);
                        var bullet2Pos = (bullet2Trajectory[0].X - b2nx, bullet2Trajectory[0].Y - b2ny);

                        if (trajectory.Contains(bullet2Pos) && bullet2Trajectory.Contains(bulletPos))
                        {
                            return new BulletCollision(otherBullet);
                        }
                    }
                }
            }

            foreach (var tank in grid.Tanks)
            {
                if (tank.IsDead)
                {
                    continue;
                }

                if (tank.X == x && tank.Y == y)
                {
                    return new TankCollision(tank);
                }

                /* Check if the bullet is on the previous position of the tank
                 * and the tank moved in front of the bullet.
                 */

                if (trajectory.Count < 2 || tank.PreviousX is null || tank.PreviousY is null)
                {
                    continue;
                }

                int tnx = tank.X - tank.PreviousX.Value;
                int tny = tank.Y - tank.PreviousY.Value;

                var (bnx, bny) = DirectionUtils.Normal(bullet.Direction);

                bool areDirectlyOpposite = bnx == -tnx && bny == -tny;
                if (areDirectlyOpposite)
                {
                    var startBulletPos = (trajectory[0].X - bnx, trajectory[0].Y - bny);
                    if (startBulletPos == (tank.X, tank.Y) && trajectory[0] == (tank.PreviousX.Value, tank.PreviousY.Value))
                    {
                        return new TankCollision(tank);
                    }
                }
            }
        }

        return null;
    }
}
