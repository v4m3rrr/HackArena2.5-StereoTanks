using System.Collections.Concurrent;

namespace GameLogic;

/// <summary>
/// Handles bullet movement, collision detection, and bullet creation.
/// </summary>
/// <param name="grid">The grid containing the bullets.</param>
/// <param name="collisionSystem">The collision system used for resolving bullet collisions.</param>
internal sealed class BulletSystem(Grid grid, BulletCollisionSystem collisionSystem)
{
    private readonly ConcurrentQueue<Bullet> queuedBullets = new();

    /// <summary>
    /// Calculates the trajectory coordinates of the bullet
    /// from the given start coordinates to the current position.
    /// </summary>
    /// <param name="bullet">The bullet to calculate the trajectory for.</param>
    /// <param name="startX">The starting x coordinate.</param>
    /// <param name="startY">The starting y coordinate.</param>
    /// <returns>
    /// A list of coordinates representing the bullet's trajectory
    /// from the start coordinates to the current position,
    /// excluding the start coordinates.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The start coordinates are not included in the returned coordinates.
    /// </para>
    /// <para>
    /// This method uses the Bresenham's line algorithm
    /// to calculate the coordinates of the bullet's trajectory.
    /// </para>
    /// </remarks>
    public static List<(int X, int Y)> CalculateTrajectory(Bullet bullet, int startX, int startY)
    {
        List<(int X, int Y)> coords = [];
        int dx = Math.Abs(bullet.X - startX);
        int dy = Math.Abs(bullet.Y - startY);
        int sx = startX < bullet.X ? 1 : -1;
        int sy = startY < bullet.Y ? 1 : -1;
        int err = dx - dy;

        if (startX == bullet.X && startY == bullet.Y)
        {
            coords.Add(new(startX, startY));
        }

        while (startX != bullet.X || startY != bullet.Y)
        {
            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                startX += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                startY += sy;
            }

            coords.Add(new(startX, startY));
        }

        return coords;
    }

    /// <summary>
    /// Attempts to fire a single bullet from the specified turret.
    /// </summary>
    /// <param name="ability">The bullet ability used to fire.</param>
    /// <returns>
    /// The fired bullet if successful; otherwise, <see langword="null"/>.
    /// </returns>
    public Bullet? TryFireBullet(BulletAbility ability)
    {
        if (!ability.CanUse)
        {
            return null;
        }

        var turret = ability.Turret;
        var tank = turret.Tank;
        var (nx, ny) = DirectionUtils.Normal(turret.Direction);
        var bulletX = tank.X + nx;
        var bulletY = tank.Y + ny;

        var bullet = new Bullet(
            bulletX,
            bulletY,
            turret.Direction,
            speed: 2.0f,
            damage: 20,
            turret.Tank.Owner);

        this.queuedBullets.Enqueue(bullet);
        ability.Use();

        return bullet;
    }

    /// <summary>
    /// Tries to fire double bullets from a turret with the double bullet ability.
    /// </summary>
    /// <param name="ability">The turret ability used to fire.</param>
    /// <returns>
    /// <see langword="true"/> if the bullets were fired; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryFireDoubleBullet(DoubleBulletAbility ability)
    {
        if (!ability.CanUse)
        {
            return false;
        }

        var turret = ability.Turret;
        var tank = turret.Tank;
        var (nx, ny) = DirectionUtils.Normal(turret.Direction);
        int bulletX = tank.X + nx;
        int bulletY = tank.Y + ny;

        var doubleBullet = new DoubleBullet(
            bulletX,
            bulletY,
            turret.Direction,
            speed: 2.0f,
            damage: 20 * 2,
            tank.Owner);

        this.queuedBullets.Enqueue(doubleBullet);
        ability.Use();

        return true;
    }

    /// <summary>
    /// Updates all active bullets on the grid by advancing their positions and resolving collisions.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="number">
    /// <item>Moves each bullet based on its velocity and direction.</item>
    /// <item>Calculates the trajectory traveled during this tick.</item>
    /// <item>Performs collision checks and resolves them.</item>
    /// <item>Adds newly queued bullets to the grid and rechecks for collisions.</item>
    /// </list>
    /// </remarks>
    public void Update(float deltaTime)
    {
        var bullets = grid.Bullets;
        var trajectories = new Dictionary<Bullet, List<(int X, int Y)>>();

        foreach (Bullet bullet in bullets)
        {
            int prevX = bullet.X;
            int prevY = bullet.Y;

            bullet.UpdatePosition(deltaTime);
            trajectories[bullet] = CalculateTrajectory(bullet, prevX, prevY);
        }

        this.ResolveBulletCollisions(trajectories);

        while (this.queuedBullets.TryDequeue(out var newBullet))
        {
            grid.Bullets.Add(newBullet);
            trajectories[newBullet] = [(newBullet.X, newBullet.Y)];
        }

        this.ResolveBulletCollisions(trajectories);
    }

    private void ResolveBulletCollisions(Dictionary<Bullet, List<(int X, int Y)>> trajectories)
    {
        foreach (Bullet bullet in grid.Bullets.ToList())
        {
            if (!grid.Bullets.Contains(bullet))
            {
                continue;
            }

            Collision? collision = CollisionDetector.CheckBulletCollision(bullet, grid, trajectories);

            if (collision is not null)
            {
                collisionSystem.ResolveCollision(bullet, collision);
            }
        }
    }
}
