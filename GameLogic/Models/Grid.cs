using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace GameLogic;

/// <summary>
/// Represents a grid.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Grid"/> class.
/// </remarks>
/// <param name="dimension">The dimension of the grid.</param>
/// <param name="seed">The seed of the grid.</param>
public class Grid(int dimension, int seed)
{
    private readonly Queue<Bullet> queuedBullets = new();
    private readonly Random random = new(seed);

    private List<Tank> tanks = [];
    private List<Bullet> bullets = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Grid"/> class.
    /// </summary>
    public Grid()
        : this(1, new Random().Next())
    {
    }

    /// <summary>
    /// Occurs when the state is updating.
    /// </summary>
    public event EventHandler? StateUpdating;

    /// <summary>
    /// Occurs when the state has updated.
    /// </summary>
    public event EventHandler? StateUpdated;

    /// <summary>
    /// Occurs when the dimensions are changing.
    /// </summary>
    public event EventHandler? DimensionsChanging;

    /// <summary>
    /// Occurs when the dimensions have changed.
    /// </summary>
    public event EventHandler? DimensionsChanged;

    /// <summary>
    /// Gets the dimension of the grid.
    /// </summary>
    public int Dim { get; private set; } = dimension;

    /// <summary>
    /// Gets the seed of the grid.
    /// </summary>
    public int Seed { get; private init; } = seed;

    /// <summary>
    /// Gets the wall grid.
    /// </summary>
    public Wall?[,] WallGrid { get; private set; } = new Wall?[dimension, dimension];

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
    public void UpdateFromStatePayload(StatePayload payload)
    {
        this.StateUpdating?.Invoke(this, EventArgs.Empty);

        var newDim = payload.WallGrid.GetLength(0);
        if (newDim != this.Dim)
        {
            this.DimensionsChanging?.Invoke(this, EventArgs.Empty);
            this.Dim = newDim;
            this.WallGrid = new Wall?[this.Dim, this.Dim];
            this.DimensionsChanged?.Invoke(this, EventArgs.Empty);
        }

        for (int i = 0; i < this.Dim; i++)
        {
            for (int j = 0; j < this.Dim; j++)
            {
                if (payload.WallGrid[i, j] is { } jObject)
                {
                    var wall = jObject.ToObject<Wall>()!;
                    wall.X = i;
                    wall.Y = j;
                    this.WallGrid[i, j] = wall;
                }
            }
        }

        this.tanks = payload.Tanks;
        this.bullets = payload.Bullets;

        this.StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Generates the walls.
    /// </summary>
    public void GenerateWalls()
    {
        var generator = new MapGenerator(this.Dim, this.Seed);
        var walls = generator.GenerateWalls();

        for (int i = 0; i < this.Dim; i++)
        {
            for (int j = 0; j < this.Dim; j++)
            {
                this.WallGrid[i, j] = walls[i, j] ? new Wall(i, j) : null;
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
            if (x >= 0 && x < this.Dim && y >= 0 && y < this.Dim && this.WallGrid[x, y] is Wall w)
            {
                neighbors[i] = w;
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Generates a tank.
    /// </summary>
    /// <param name="owner">The owner of the tank.</param>
    /// <returns>The generated tank.</returns>
    public Tank GenerateTank(Player owner)
    {
        var (x, y) = this.GetRandomEmptyCell();

        var tank = new Tank(x, y, owner);
        this.tanks.Add(tank);

        tank.Turret.Shot += this.queuedBullets.Enqueue;
        tank.Regenerated += (s, e) =>
        {
            var (x, y) = this.GetRandomEmptyCell();
            tank.SetPosition(x, y);
        };

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
    /// Regenerates the tanks.
    /// </summary>
    /// <remarks>
    /// This method regenerates the dead tanks
    /// by calling the <see cref="Tank.Regenerate"/> method.
    /// </remarks>
    public void RegenerateTanks()
    {
        this.tanks.ForEach(x => x.Regenerate());
    }

    /// <summary>
    /// Updates the bullets.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void UpdateBullets(float deltaTime)
    {
        Dictionary<Bullet, List<(int X, int Y)>> trajectories = [];

        foreach (Bullet bullet in this.bullets.ToList())
        {
            int previousX = bullet.X;
            int previousY = bullet.Y;
            bullet.UpdatePosition(deltaTime);
            trajectories[bullet] = bullet.CalculateTrajectory(previousX, previousY);
        }

        List<Bullet> destroyedBullets = [];
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

        while (this.queuedBullets.Count > 0)
        {
            var bullet = this.queuedBullets.Dequeue();

            if (bullet is null)
            {
                continue;
            }

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
    public StatePayload ToPayload()
    {
        var wallGrid = new JObject?[Dim, Dim];
        foreach (Wall? wall in this.WallGrid.Cast<Wall>().Where(w => w is not null))
        {
            wallGrid[wall.X, wall.Y] = JObject.FromObject(wall);
        }

        return new StatePayload
        {
            WallGrid = wallGrid,
            Tanks = this.tanks,
            Bullets = this.bullets,
        };
    }

    private bool IsCellEmpty(int x, int y)
    {
        return this.WallGrid[x, y] is null && !this.tanks.Any(t => t.X == x && t.Y == y && !t.IsDead);
    }

    private List<Bullet> HandleBulletCollision(Bullet bullet, ICollision collision)
    {
        var destroyedBullets = new List<Bullet> { bullet };
        _ = this.bullets.Remove(bullet);

        switch (collision)
        {
            case TankCollision tankCollision:
                bullet.Shooter.Score += bullet.Damage / 2;
                tankCollision.Tank.TakeDamage(bullet.Damage);
                break;

            case BulletCollision bulletCollision:
                _ = this.bullets.Remove(bulletCollision.Bullet);
                destroyedBullets.Add(bulletCollision.Bullet);
                break;
        }

        return destroyedBullets;
    }

    private (int X, int Y) GetRandomEmptyCell()
    {
        int x, y;
        do
        {
            x = this.random.Next(Dim);
            y = this.random.Next(Dim);
        }
        while (!this.IsCellEmpty(x, y));

        return (x, y);
    }

    /// <summary>
    /// Represents the grid state payload.
    /// </summary>
    public class StatePayload
    {
        /// <summary>
        /// Gets the wall grid.
        /// </summary>
        public JObject?[,] WallGrid { get; init; } = new JObject?[0, 0];

        /// <summary>
        /// Gets the tanks.
        /// </summary>
        public List<Tank> Tanks { get; init; } = [];

        /// <summary>
        /// Gets the bullets.
        /// </summary>
        public List<Bullet> Bullets { get; init; } = [];
    }
}
