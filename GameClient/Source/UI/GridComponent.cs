using GameClient.Networking;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents the grid component.
/// Handles layout, update and draw loops, and delegates synchronization.
/// </summary>
internal class GridComponent : Component
{
    private readonly List<ISyncService> syncServices = [];

    private readonly List<Sprites.Tank> tanks = [];
    private readonly List<Sprites.Bullet> bullets = [];
    private readonly List<Sprites.Zone> zones = [];
    private readonly List<Sprites.Laser> lasers = [];
    private readonly List<Sprites.Mine> mines = [];
    private readonly List<Sprites.RadarEffect> radarEffects = [];
#if !STEREO
    private readonly List<Sprites.SecondaryItem> mapItems = [];
#endif
    private readonly List<Sprites.FogOfWar> fogsOfWar = [];

    private Sprites.Wall.WallWithLogic?[,] walls;
    private List<Sprites.Wall.Border> borderWalls = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GridComponent"/> class.
    /// </summary>
    public GridComponent()
    {
        this.Logic = Grid.Empty;
        this.walls = new Sprites.Wall.WallWithLogic?[0, 0];

        this.Logic.DimensionsChanged += this.OnDimensionsChanged;
        this.Transform.SizeChanged += (s, e) => this.UpdateDrawData();

#if !STEREO
        this.syncServices.Add(new MapItemSyncService(this, this.mapItems));
#endif
        this.syncServices.Add(new TankSyncService(this,  this.tanks));
        this.syncServices.Add(new BulletSyncService(this, this.bullets));
        this.syncServices.Add(new LaserSyncService(this, this.lasers));
        this.syncServices.Add(new MineSyncService(this, this.mines));
        this.syncServices.Add(new ZoneSyncService(this, this.zones));
        this.syncServices.Add(new WallSyncService(this, () => this.walls, walls => this.walls = walls, this.borderWalls));
        this.syncServices.Add(new RadarSyncService(this, this.radarEffects));
        this.syncServices.Add(new FogOfWarSyncService(this, this.fogsOfWar));
    }

    /// <summary>
    /// Occurs when the draw data has changed.
    /// </summary>
    public event EventHandler? DrawDataChanged;

    /// <summary>
    /// Gets the grid logic.
    /// </summary>
    public Grid Logic { get; private set; }

    /// <summary>
    /// Gets the tile size in pixels.
    /// </summary>
    public int TileSize { get; private set; }

    /// <summary>
    /// Gets the pixel offset to center the grid.
    /// </summary>
    public int DrawOffset { get; private set; }

    /// <summary>
    /// Gets all sprites (tanks, bullets, zones, etc.) for rendering.
    /// </summary>
    public IEnumerable<ISprite> AllSprites
    {
        get
        {
            lock (this)
            {
                return this.fogsOfWar.Cast<ISprite>()
                    .Concat(this.zones)
#if !STEREO
                    .Concat(this.mapItems)
#endif
                    .Concat(this.mines)
                    .Concat(this.tanks)
                    .Concat(this.bullets)
                    .Concat(this.lasers)
                    .Concat(this.radarEffects)
                    .Concat(this.walls.Cast<ISprite>().Where(x => x is not null))
                    .Concat(this.borderWalls.Cast<ISprite>())
                    .Where(x => x is not null)!;
            }
        }
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        lock (this)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            base.Update(gameTime);

            foreach (ISprite sprite in this.AllSprites.ToList())
            {
                sprite.Update(gameTime);
            }
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        lock (this)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            base.Draw(gameTime);

            foreach (ISprite sprite in this.AllSprites.ToList())
            {
                sprite.Draw(gameTime);
            }
        }
    }

    /// <summary>
    /// Synchronizes all visual subsystems with the current game logic state.
    /// </summary>
    /// <remarks>
    /// This method sequentially invokes all registered <see cref="ISyncService"/> implementations
    /// to update the corresponding sprites (e.g., tanks, bullets, fog of war, walls).
    /// It is thread-safe and protected by an internal synchronization lock.
    /// </remarks>
    public void Sync()
    {
#if WINDOWS

        lock (this)
        {
            foreach (var service in this.syncServices)
            {
                service.Sync();
            }
        }

#else

        GameClientCore.InvokeOnMainThread(() =>
        {
            foreach (var service in this.syncServices)
            {
                service.Sync();
            }
        });

#endif
    }

    /// <summary>
    /// Clears all rendered sprite data from the client-side grid,
    /// resetting all visual elements to an empty state.
    /// </summary>
    /// <remarks>
    /// The operation is thread-safe
    /// and protected by an internal synchronization lock.
    /// </remarks>
    public void ClearSprites()
    {
        lock (this)
        {
            this.tanks.Clear();
            this.bullets.Clear();
            this.zones.Clear();
            this.lasers.Clear();
            this.mines.Clear();
            this.radarEffects.Clear();
#if !STEREO
            this.mapItems.Clear();
#endif
            this.fogsOfWar.Clear();
            this.walls = new Sprites.Wall.WallWithLogic?[this.Logic.Dim, this.Logic.Dim];
        }
    }

    private void OnDimensionsChanged(object? sender, EventArgs args)
    {
        this.UpdateDrawData();

        this.walls = new Sprites.Wall.WallWithLogic?[this.Logic.Dim, this.Logic.Dim];

        var borderWalls = new List<Sprites.Wall.Border>();
        for (int i = 0; i < this.Logic.Dim; i++)
        {
            borderWalls.Add(new Sprites.Wall.Border(i, -1, this));
            borderWalls.Add(new Sprites.Wall.Border(i, this.Logic.Dim, this));
            borderWalls.Add(new Sprites.Wall.Border(-1, i, this));
            borderWalls.Add(new Sprites.Wall.Border(this.Logic.Dim, i, this));
        }

        borderWalls.Add(new Sprites.Wall.Border(-1, -1, this));
        borderWalls.Add(new Sprites.Wall.Border(-1, this.Logic.Dim, this));
        borderWalls.Add(new Sprites.Wall.Border(this.Logic.Dim, -1, this));
        borderWalls.Add(new Sprites.Wall.Border(this.Logic.Dim, this.Logic.Dim, this));

        this.borderWalls = borderWalls;
    }

    private void UpdateDrawData()
    {
        if (this.Logic.Dim == 0)
        {
            return;
        }

        var size = this.Transform.Size.X;
        var tileSize = this.TileSize = size / this.Logic.Dim;
        this.DrawOffset = (int)((size - (this.Logic.Dim * tileSize)) / 2f);
        this.DrawDataChanged?.Invoke(this, EventArgs.Empty);
    }
}
