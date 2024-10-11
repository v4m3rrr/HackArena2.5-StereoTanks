using System.Diagnostics;

namespace GameLogic;

/// <summary>
/// Represents a grid.
/// </summary>
/// <param name="dimension">The dimension of the grid.</param>
/// <param name="seed">The seed of the grid.</param>
public class Grid(int dimension, int seed)
{
    private readonly Queue<Bullet> queuedBullets = new();
    private readonly Random random = new(seed);
    private readonly MapGenerator generator = new(dimension, seed);

    private List<Zone> zones = [];
    private List<Tank> tanks = [];
    private List<Bullet> bullets = [];
    private List<SecondaryMapItem> items = [];

    private FogOfWarManager fogOfWarManager = new(new bool[0, 0]);

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
    /// Gets an empty grid.
    /// </summary>
    public static Grid Empty => new(0, 0);

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
    /// Gets the zones.
    /// </summary>
    public IEnumerable<Zone> Zones => this.zones;

    /// <summary>
    /// Gets the tanks.
    /// </summary>
    public IEnumerable<Tank> Tanks => this.tanks;

    /// <summary>
    /// Gets the bullets.
    /// </summary>
    public IEnumerable<Bullet> Bullets => this.bullets;

    /// <summary>
    /// Gets the items.
    /// </summary>
    public IEnumerable<SecondaryMapItem> Items => this.items;

    /// <summary>
    /// Updates the grid state from a payload.
    /// </summary>
    /// <param name="payload">The payload to update from.</param>
    /// <remarks>
    /// <para>
    /// This method performs the following property settings:
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="Player.Tank"/> is set for each player based on the payload.
    /// </description></item>
    /// <item><description>
    /// <see cref="Tank.Owner"/> is set for each tank based on the payload.
    /// </description></item>
    /// <item><description>
    /// <see cref="Turret.Tank"/> is set for each turret based on its associated tank.
    /// </description></item>
    /// <item><description>
    /// <see cref="Bullet.Shooter"/> is set for each bullet based on the payload.
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method raises the <see cref="StateUpdating"/> event before updating the state,
    /// and the <see cref="StateUpdated"/> event after updating the state.
    /// </para>
    /// <para>
    /// If the dimensions of the grid have changed, this method raises the
    /// <see cref="DimensionsChanging"/>  event before updating the dimensions,
    /// and the <see cref="DimensionsChanged"/> event after updating the dimensions.
    /// </para>
    /// </remarks>
    public void UpdateFromGameStatePayload(Networking.GameStatePayload payload)
    {
        this.StateUpdating?.Invoke(this, EventArgs.Empty);

        var map = payload.Map;
        var tiles = map.Tiles;

        // Update the dimensions of the grid.
        var newDim = tiles.WallGrid.GetLength(0);
        if (newDim != this.Dim)
        {
            this.DimensionsChanging?.Invoke(this, EventArgs.Empty);
            this.Dim = newDim;
            this.WallGrid = new Wall?[this.Dim, this.Dim];
            this.DimensionsChanged?.Invoke(this, EventArgs.Empty);
        }

        // Update the walls.
        this.WallGrid = tiles.WallGrid;

        // Update the tanks.
        this.tanks = [.. tiles.Tanks];

        // Set the tanks' owners, owners' tanks and turrets' tanks.
        foreach (Tank tank in this.tanks)
        {
            var owner = payload.Players.First(p => p.Id == tank.OwnerId);

            tank.Owner = owner;
            tank.Turret.Tank = tank;
            owner.Tank = tank;
        }

        // Update the bullets.
        this.bullets = tiles.Bullets;

        // Set the bullets' shooters.
        foreach (Bullet bullet in this.bullets)
        {
            if (bullet.ShooterId is null)
            {
                // ShooterId is null for players.
                // Only spectators have bullets with ShooterId.
                Debug.Assert(
                    payload is Networking.GameStatePayload.ForPlayer,
                    "Bullet shooter ID is null for player payload.");
                continue;
            }

            var shooter = payload.Players.First(p => p.Id == bullet.ShooterId);
            bullet.Shooter = shooter;
        }

        // Update the zones.
        this.zones = map.Zones;

        Player player;
        foreach (Zone zone in this.zones)
        {
            switch (zone.Status)
            {
                case ZoneStatus.BeingCaptured beingCaptured:
                    player = payload.Players.First(p => p.Id == beingCaptured.PlayerId);
                    beingCaptured.Player = player;
                    break;

                case ZoneStatus.Captured captured:
                    player = payload.Players.First(p => p.Id == captured.PlayerId);
                    captured.Player = player;
                    break;

                case ZoneStatus.BeingContested beingContested:
                    if (beingContested.CapturedById is not null)
                    {
                        player = payload.Players.First(p => p.Id == beingContested.CapturedById);
                        beingContested.CapturedBy = player;
                    }

                    break;

                case ZoneStatus.BeingRetaken beingRetaken:
                    player = payload.Players.First(p => p.Id == beingRetaken.CapturedById);
                    var player2 = payload.Players.First(p => p.Id == beingRetaken.RetakenById);
                    beingRetaken.CapturedBy = player;
                    beingRetaken.RetakenBy = player2;
                    break;
            }
        }

        // Update the items.
        this.items = [.. tiles.Items];

        this.StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Generates the map.
    /// </summary>
    public void GenerateMap()
    {
        var walls = this.generator.GenerateWalls();
        this.zones = this.generator.GenerateZones();

        this.generator.RemoveSomeWallsFromZones(walls, this.zones);

        this.fogOfWarManager = new FogOfWarManager(walls);

        for (int x = 0; x < this.Dim; x++)
        {
            for (int y = 0; y < this.Dim; y++)
            {
                this.WallGrid[x, y] = walls[x, y] ? new Wall() { X = x, Y = y } : null;
            }
        }
    }

    /// <summary>
    /// Generates a new item on the map.
    /// </summary>
    public void GenerateNewItemOnMap()
    {
        var doubleBulletProbability = 0.0009;

        if (this.random.NextDouble() > doubleBulletProbability)
        {
            return;
        }

        int x, y;
        int triesLeft = 100;
        do
        {
            (x, y) = this.GetRandomEmptyCell();
        } while ((this.GetCellObjects(x, y).Any() || this.IsVisibleByTank(x, y)) && triesLeft-- > 0);

        if (triesLeft > 0)
        {
            this.items.Add(new SecondaryMapItem(x, y, SecondaryItemType.DoubleBullet));
        }
    }

    /// <summary>
    /// Generates a tank.
    /// </summary>
    /// <param name="owner">The owner of the tank.</param>
    /// <returns>The generated tank.</returns>
    public Tank GenerateTank(Player owner)
    {
        var (x, y) = this.GetRandomEmptyCell();

        var tankDirection = EnumUtils.Random<Direction>(this.random);
        var turretDirection = EnumUtils.Random<Direction>(this.random);

        var tank = new Tank(x, y, tankDirection, turretDirection, owner);

        owner.Tank = tank;
        this.tanks.Add(tank);

        tank.Turret.Shot += this.queuedBullets.Enqueue;
        owner.TankRegenerated += (s, e) =>
        {
            var (x, y) = this.GetRandomEmptyCell();
            tank.SetPosition(x, y);
        };

        return tank;
    }

    /// <summary>
    /// Removes a tank.
    /// </summary>
    /// <param name="owner">The owner of the tank to remove.</param>
    /// <returns>The removed tank if found; otherwise, <c>null</c>.</returns>
    public Tank? RemoveTank(Player owner)
    {
        var tank = this.tanks.FirstOrDefault(t => t.Owner == owner);

        if (tank is not null)
        {
            _ = this.tanks.Remove(tank);
        }

        return tank;
    }

    /// <summary>
    /// Tries to move a tank.
    /// </summary>
    /// <param name="tank">The tank to move.</param>
    /// <param name="movement">The movement direction.</param>
    public void TryMoveTank(Tank tank, MovementDirection movement)
    {
        var (dx, dy) = DirectionUtils.Normal(tank.Direction);
        int step = -((int)movement * 2) + 1;
        int x = tank.X + (dx * step);
        int y = tank.Y + (dy * step);

        if (!this.GetCellObjects(x, y).Any(x => x is Wall or Tank))
        {
            tank.SetPosition(x, y);
        }
    }

    /// <summary>
    /// Updates the tanks' regeneration progress.
    /// </summary>
    /// <remarks>
    /// This method regenerates the dead tanks
    /// by calling the <see cref="Player.UpdateRegenerationProgress"/> method.
    /// </remarks>
    internal void UpdateTanksRegenerationProgress()
    {
        this.Tanks
            .Select(x => x.Owner)
            .ToList()
            .ForEach(x => x.UpdateRegenerationProgress());
    }

    /// <summary>
    /// Updates the bullets.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    internal void UpdateBullets(float deltaTime)
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
    /// Regenerates the players' bullets.
    /// </summary>
    internal void RegeneratePlayersBullets()
    {
        foreach (Tank tank in this.tanks)
        {
            tank.Turret.RegenerateBullets();
        }
    }

    /// <summary>
    /// Updates the players' visibility grids.
    /// </summary>
    internal void UpdatePlayersVisibilityGrids()
    {
        foreach (Tank tank in this.tanks)
        {
            tank.Owner.CalculateVisibilityGrid(this.fogOfWarManager);
        }
    }

    /// <summary>
    /// Updates the zones.
    /// </summary>
    internal void UpdateZones()
    {
        foreach (Zone zone in this.zones)
        {
            zone.UpdateCapturingStatus(this.tanks);
        }
    }

    /// <summary>
    /// Picks up the items that are on the same cell as the tanks
    /// and sets the tanks' secondary item type.
    /// </summary>
    internal void PickUpItems()
    {
        foreach (Tank tank in this.tanks)
        {
            if (tank.SecondaryItemType is not null)
            {
                continue;
            }

            SecondaryMapItem? item = this.items.FirstOrDefault(i => i.X == tank.X && i.Y == tank.Y);

            if (item is null)
            {
                continue;
            }

            tank.SecondaryItemType = item.Type;
            _ = this.items.Remove(item);
        }
    }

    /// <summary>
    /// Converts the grid to a map payload.
    /// </summary>
    /// <param name="player">The player to convert the grid for.</param>
    /// <returns>The map payload for the grid.</returns>
    internal MapPayload ToMapPayload(Player? player)
    {
        var visibility = player is not null
            ? new VisibilityPayload(player!.VisibilityGrid!)
            : null;
        var tiles = new TilesPayload(this.WallGrid, this.tanks, this.bullets, this.items);
        return new MapPayload(visibility, tiles, this.zones);
    }

    private bool IsVisibleByTank(int x, int y)
    {
        return this.tanks.Any(t => t.Owner.VisibilityGrid is not null && t.Owner.VisibilityGrid[x, y]);
    }

    private IEnumerable<object> GetCellObjects(int x, int y)
    {
        Wall? wall = this.WallGrid[x, y];
        if (wall is not null)
        {
            yield return wall;
        }

        Tank? tank = this.tanks.FirstOrDefault(t => t.X == x && t.Y == y);
        if (tank is not null)
        {
            yield return tank;
        }

        Bullet? bullet = this.bullets.FirstOrDefault(b => b.X == x && b.Y == y);
        if (bullet is not null)
        {
            yield return bullet;
        }

        IEnumerable<SecondaryMapItem> items = this.items.Where(i => i.X == x && i.Y == y);
        foreach (SecondaryMapItem item in items)
        {
            yield return item;
        }
    }

    private bool IsCellWithinBounds(int x, int y)
    {
        return x >= 0 && x < this.Dim && y >= 0 && y < this.Dim;
    }

    private List<Bullet> HandleBulletCollision(Bullet bullet, ICollision collision)
    {
        var destroyedBullets = new List<Bullet>();
        _ = this.bullets.Remove(bullet);

        switch (collision)
        {
            case TankCollision tankCollision:
                destroyedBullets.Add(bullet);
                bullet.Shooter!.Score += bullet.Damage!.Value / 2;
                tankCollision.Tank.TakeDamage(bullet.Damage.Value);
                if (tankCollision.Tank.IsDead)
                {
                    bullet.Shooter.Kills++;
                }

                break;

            case BulletCollision bulletCollision:
                _ = this.bullets.Remove(bulletCollision.Bullet);
                destroyedBullets.Add(bulletCollision.Bullet);
                destroyedBullets.Add(bullet);

                if (!(bullet is DoubleBullet ^ bulletCollision.Bullet is DoubleBullet))
                {
                    break;
                }

                var bulletToDowngrade = bullet is DoubleBullet
                    ? bullet
                    : bulletCollision.Bullet;

                var newBullet = new Bullet(
                    bulletToDowngrade.X,
                    bulletToDowngrade.Y,
                    bulletToDowngrade.Direction,
                    bulletToDowngrade.Speed,
                    bulletToDowngrade.Damage!.Value / 2,
                    bulletToDowngrade.Shooter!);

                this.bullets.Add(newBullet);

                break;
        }

        return destroyedBullets;
    }

    private (int X, int Y) GetRandomEmptyCell(int? maxTries = 100)
    {
        int x, y;
        do
        {
            x = this.random.Next(this.Dim);
            y = this.random.Next(this.Dim);
        }
        while (maxTries-- > 0 && (!this.IsCellWithinBounds(x, y) || this.GetCellObjects(x, y).Any()));

        return maxTries > 0
            ? ((int X, int Y))(x, y)
            : throw new InvalidOperationException("Failed to find an empty cell.");
    }

    /// <summary>
    /// Represents a map payload for the grid.
    /// </summary>
    /// <param name="Visibility">The visibility payload for the grid.</param>
    /// <param name="Tiles">The tiles payload for the grid.</param>
    /// <param name="Zones">The zones of the grid.</param>
    internal record class MapPayload(VisibilityPayload? Visibility, TilesPayload Tiles, List<Zone> Zones);

    /// <summary>
    /// Represents a tiles payload for the grid.
    /// </summary>
    /// <param name="WallGrid">The wall grid of the grid.</param>
    /// <param name="Tanks">The tanks of the grid.</param>
    /// <param name="Bullets">The bullets of the grid.</param>
    /// <param name="Items">The items on the grid.</param>
    internal record class TilesPayload(Wall?[,] WallGrid, List<Tank> Tanks, List<Bullet> Bullets, List<SecondaryMapItem> Items);

    /// <summary>
    /// Represents a visibility payload for the grid.
    /// </summary>
    /// <param name="VisibilityGrid">The visibility grid of the grid.</param>
    internal record class VisibilityPayload(bool[,] VisibilityGrid);
}
