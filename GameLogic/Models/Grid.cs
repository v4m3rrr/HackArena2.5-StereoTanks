using GameLogic.Networking;
using GameLogic.Networking.Map;

namespace GameLogic;

/// <summary>
/// Represents the logical state of the game grid, including map layout and all active entities.
/// </summary>
/// <param name="dimension">The dimension of the grid.</param>
/// <param name="seed">The seed used for random generation.</param>
internal class Grid(int dimension, int seed)
{
    /// <summary>
    /// Occurs when the grid state is about to be updated.
    /// </summary>
    public event EventHandler? StateUpdating;

    /// <summary>
    /// Occurs when the grid state has been updated.
    /// </summary>
    public event EventHandler? StateUpdated;

    /// <summary>
    /// Occurs before the grid dimensions are about to change.
    /// </summary>
    public event EventHandler? DimensionsChanging;

    /// <summary>
    /// Occurs after the grid dimensions have changed.
    /// </summary>
    public event EventHandler? DimensionsChanged;

    /// <summary>
    /// Gets an empty grid instance.
    /// </summary>
    public static Grid Empty => new(0, 0);

    /// <summary>
    /// Gets or sets the dimension (width and height) of the grid.
    /// </summary>
    public int Dim { get; set; } = dimension;

    /// <summary>
    /// Gets the seed used to initialize the grid.
    /// </summary>
    public int Seed { get; } = seed;

    /// <summary>
    /// Gets or sets the wall grid layout.
    /// </summary>
    public Wall?[,] WallGrid { get; set; } = new Wall?[dimension, dimension];

    /// <summary>
    /// Gets the shared random generator used by this grid.
    /// </summary>
    public Random Random { get; } = new Random(seed);

    /// <summary>
    /// Gets the tank list.
    /// </summary>
    public List<Tank> Tanks { get; } = [];

    /// <summary>
    /// Gets the bullet list.
    /// </summary>
    public List<Bullet> Bullets { get; } = [];

    /// <summary>
    /// Gets the zone list.
    /// </summary>
    public List<Zone> Zones { get; } = [];

    /// <summary>
    /// Gets the laser list.
    /// </summary>
    public List<Laser> Lasers { get; } = [];

    /// <summary>
    /// Gets the mine list.
    /// </summary>
    public List<Mine> Mines { get; } = [];

#if !STEREO

    /// <summary>
    /// Gets the secondary item list.
    /// </summary>
    public List<SecondaryItem> SecondaryItems { get; } = [];

#endif

#if CLIENT

    /// <summary>
    /// Gets the tank of the player with the given ID.
    /// </summary>
    /// <param name="playerId">The ID of the player whose tank is to be retrieved.</param>
    /// <returns>
    /// The tank of the player with the given ID,
    /// or <see langword="null"/> if not found.
    /// </returns>
    public Tank? GetPlayerTank(string playerId)
    {
        return this.Tanks.FirstOrDefault(t => t.OwnerId == playerId);
    }

#endif

    /// <summary>
    /// Raises the <see cref="StateUpdating"/> event.
    /// </summary>
    public void OnStateUpdating()
    {
        this.StateUpdating?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="StateUpdated"/> event.
    /// </summary>
    public void OnStateUpdated()
    {
        this.StateUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="DimensionsChanging"/> event.
    /// </summary>
    public void OnDimensionsChanging()
    {
        this.DimensionsChanging?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="DimensionsChanged"/> event.
    /// </summary>
    public void OnDimensionsChanged()
    {
        this.DimensionsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Determines whether the given coordinates are within the grid bounds.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns><see langword="true"/> if inside bounds; otherwise, <see langword="false"/>.</returns>
    internal bool IsCellWithinBounds(int x, int y)
    {
        return x >= 0 && x < this.Dim && y >= 0 && y < this.Dim;
    }

    /// <summary>
    /// Converts the current grid state into a serializable map payload.
    /// </summary>
    /// <param name="player">The player for which visibility should be calculated.</param>
    /// <returns>The map payload.</returns>
    internal MapPayload ToMapPayload(Player? player)
    {
        var tiles = new TilesPayload(this.WallGrid, this.Tanks, this.Bullets, this.Lasers, this.Mines)
#if !STEREO
        {
            Items = this.SecondaryItems,
        }
#endif
        ;

        return new MapPayload(tiles, this.Zones)
        {
#if !STEREO
            Visibility = player is null ? null : new VisibilityPayload(player.Tank.VisibilityGrid!),
#endif
        };
    }

    /// <summary>
    /// Gets all objects located at the specified cell.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The collection of objects.</returns>
    internal IEnumerable<object> GetCellObjects(int x, int y)
    {
        if (this.WallGrid[x, y] is Wall wall)
        {
            yield return wall;
        }

        if (this.Tanks.FirstOrDefault(t => t.X == x && t.Y == y) is Tank tank)
        {
            yield return tank;
        }

        if (this.Bullets.FirstOrDefault(b => b.X == x && b.Y == y) is Bullet bullet)
        {
            yield return bullet;
        }

        if (this.Lasers.FirstOrDefault(l => l.X == x && l.Y == y) is Laser laser)
        {
            yield return laser;
        }

        if (this.Mines.FirstOrDefault(m => m.X == x && m.Y == y) is Mine mine)
        {
            yield return mine;
        }

#if !STEREO
        foreach (var item in this.SecondaryItems.Where(i => i.X == x && i.Y == y))
        {
            yield return item;
        }
#endif
    }
}
