using System.Numerics;
using GameLogic.Networking;

namespace GameLogic;

/// <summary>
/// Represents a grid.
/// </summary>
public class Grid
{
    /// <summary>
    /// The dimension of the grid.
    /// </summary>
    public const int Dim = 25;

    /// <summary>
    /// The number of inner walls.
    /// </summary>
    public const int InnerWalls = 5000;

    private static readonly Random Random = new();

    private readonly Queue<Bullet> bulletsQueue = new();

    private List<Tank> tanks = new();
    private List<Bullet> bullets = new();

    /// <summary>
    /// Occurs when the state is updating.
    /// </summary>
    public event EventHandler? StateUpdating;

    /// <summary>
    /// Occurs when the state has updated.
    /// </summary>
    public event EventHandler? StateUpdated;

    /// <summary>
    /// Gets the wall grid.
    /// </summary>
    public Wall?[,] WallGrid { get; private set; } = new Wall?[Dim, Dim];

    /// <summary>
    /// Gets the tanks.
    /// </summary>
    public IEnumerable<Tank> Tanks => this.tanks;

    /// <summary>
    /// Gets the bullets.
    /// </summary>
    public IEnumerable<Bullet> Bullets => this.bullets;

    /// <summary>
    /// Updates the grid state from a payload.
    /// </summary>
    /// <param name="payload">The payload to update from.</param>
    public void UpdateFromPayload(GridStatePayload payload)
    {
        this.StateUpdating?.Invoke(this, EventArgs.Empty);
        this.WallGrid = payload.WallGrid;
        this.tanks = payload.Tanks;
        this.bullets = payload.Bullets;
        this.StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Generates the walls.
    /// </summary>
    public void GenerateWalls()
    {
        var walls = MapGenerator.GenerateWalls();

        for (int i = 0; i < Dim; i++)
        {
            for (int j = 0; j < Dim; j++)
            {
                this.WallGrid[i, j] = walls[i, j] ? new Wall() { X = i, Y = j } : null;
            }
        }
    }

    /// <summary>
    /// Gets the neighbors of a wall.
    /// </summary>
    /// <param name="wall">The wall to get the neighbors of.</param>
    /// <returns>The neighbors of the wall.</returns>
    public Wall?[] GetWallNeighbors(Wall wall)
    {
        var neighbors = new Wall?[4];
        var directions = new (int Dx, int Dy)[4] { (1, 0), (0, 1), (-1, 0), (0, -1) };

        for (int i = 0; i < directions.Length; i++)
        {
            int x = wall.X - directions[i].Dx;
            int y = wall.Y - directions[i].Dy;
            if (x >= 0 && x < Dim && y >= 0 && y < Dim && this.WallGrid[x, y] is Wall w)
            {
                neighbors[i] = w;
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Generates a tank.
    /// </summary>
    /// <returns>The generated tank.</returns>
    public Tank GenerateTank()
    {
        int x;
        int y;

        do
        {
            x = Random.Next(1, Dim - 1);
            y = Random.Next(1, Dim - 1);
        }
        while (!this.IsCellEmpty(x, y));

        var tank = new Tank(x, y)
        {
            Color = (uint)(255 << 24 | (uint)Random.Next(0xFFFFFF)),
        };
        tank.OnShoot += this.bulletsQueue.Enqueue;
        this.tanks.Add(tank);

        return tank;
    }

    /// <summary>
    /// Tries to move a tank.
    /// </summary>
    /// <param name="tank">The tank to move.</param>
    /// <param name="movement">The movement direction.</param>
    public void TryMoveTank(Tank tank, TankMovement movement)
    {
        var (dx, dy) = DirectionUtils.Normal(tank.Direction);
        int step = -((int)movement * 2) + 1;
        int x = tank.X + (dx * step);
        int y = tank.Y + (dy * step);

        if (this.IsCellEmpty(x, y))
        {
            tank.SetPosition(x, y);
        }
    }

    /// <summary>
    /// Updates the bullets.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void UpdateBullets(float deltaTime)
    {
        Dictionary<Bullet, List<(int X, int Y)>> trajectories = new();

        foreach (Bullet bullet in this.bullets.ToList())
        {
            int previousX = bullet.X;
            int previousY = bullet.Y;
            bullet.UpdatePosition(deltaTime);
            trajectories[bullet] = bullet.CalculateTrajectory(previousX, previousY);
        }

        List<Bullet> destroyedBullets = new();
        foreach (Bullet bullet in this.bullets.ToList())
        {
            if (destroyedBullets.Contains(bullet))
            {
                continue;
            }

            ICollision? collision = CollisionDetector.CheckBulletCollision(bullet, this, trajectories);

            if (collision is not null)
            {
                destroyedBullets.AddRange(this.HandleBulletCollision(bullet, collision));
            }
        }

        while (this.bulletsQueue.Count > 0)
        {
            var bullet = this.bulletsQueue.Dequeue();
            this.bullets.Add(bullet);
            trajectories.Add(bullet, [(bullet.X, bullet.Y)]);

            ICollision? collision = CollisionDetector.CheckBulletCollision(bullet, this, trajectories);

            if (collision is not null)
            {
                _ = this.HandleBulletCollision(bullet, collision);
            }
        }
    }

    /// <summary>
    /// Converts the grid to a payload.
    /// </summary>
    /// <returns>The payload representing the grid.</returns>
    public GridStatePayload ToPayload()
    {
        return new GridStatePayload
        {
            WallGrid = this.WallGrid,
            Tanks = this.tanks,
            Bullets = this.bullets,
        };
    }

    private bool IsCellEmpty(int x, int y)
    {
        return this.WallGrid[x, y] is null && !this.tanks.Any(t => t.X == x && t.Y == y);
    }

    private List<Bullet> HandleBulletCollision(Bullet bullet, ICollision collision)
    {
        var destroyedBullets = new List<Bullet> { bullet };
        _ = this.bullets.Remove(bullet);

        switch (collision)
        {
            case TankCollision tankCollision:
                tankCollision.Tank.TakeDamage(10);
                break;
            case BulletCollision bulletCollision:
                _ = this.bullets.Remove(bulletCollision.Bullet);
                destroyedBullets.Add(bulletCollision.Bullet);
                break;
        }

        return destroyedBullets;
    }
}
