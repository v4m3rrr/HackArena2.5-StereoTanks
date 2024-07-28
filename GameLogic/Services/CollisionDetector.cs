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
    public static ICollision? CheckBulletCollision(
        Bullet bullet,
        Grid grid,
        Dictionary<Bullet, List<(int X, int Y)>> trajectories)
    {
        var trajectory = trajectories[bullet];

        for (int i = 0; i < trajectories.First().Value.Count; i++)
        {
            if (i >= trajectory.Count)
            {
                break;
            }

            var (x, y) = trajectory[i];

            Wall? wall = grid.WallGrid[x, y];
            if (wall is not null)
            {
                return new WallCollision(wall);
            }

            foreach (var otherBullet in trajectories.Keys.Where(x => x != bullet))
            {
                if (trajectories.TryGetValue(otherBullet, out var bullet2Trajectory))
                {
                    bool arePerpendicular = DirectionUtils.ArePerpendicular(bullet.Direction, otherBullet.Direction);
                    if ((arePerpendicular && i < bullet2Trajectory.Count && bullet2Trajectory[i].X == x && bullet2Trajectory[i].Y == y)
                        || (!arePerpendicular && bullet2Trajectory.Any(c => c.X == x && c.Y == y)))
                    {
                        return new BulletCollision(otherBullet);
                    }
                }
            }

            foreach (var tank in grid.Tanks)
            {
                if (!tank.Equals(bullet.Shooter) && tank.X == x && tank.Y == y)
                {
                    return new TankCollision(tank);
                }
            }
        }

        return null;
    }
}
