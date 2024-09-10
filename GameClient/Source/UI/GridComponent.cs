using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents the grid component.
/// </summary>
internal class GridComponent : Component
{
    private readonly List<Sprites.Tank> tanks = [];
    private readonly List<Sprites.Bullet> bullets = [];
    private readonly List<Sprites.Zone> zones = [];

    private Sprites.FogOfWar? fogOfWar;
    private Sprites.Wall.Solid?[,] solidWalls;
    private List<Sprites.Wall.Border> borderWalls = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="GridComponent"/> class.
    /// </summary>
    public GridComponent()
    {
        this.Logic = Grid.Empty;
        this.solidWalls = new Sprites.Wall.Solid?[0, 0];
        this.Logic.DimensionsChanged += this.Logic_DimensionsChanged;
        this.Logic.StateUpdated += this.Logic_StateDeserialized;
        this.Transform.SizeChanged += (s, e) => this.UpdateDrawData();
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
    /// Gets the tile size.
    /// </summary>
    /// <value>The tile size in pixels.</value>
    public int TileSize { get; private set; }

    /// <summary>
    /// Gets the draw offset.
    /// </summary>
    /// <value>The draw offset in pixels to center the grid.</value>
    public int DrawOffset { get; private set; }

    private IEnumerable<Sprite> Sprites => this.zones.Cast<Sprite>()
        .Concat(this.tanks)
        .Concat(this.bullets)
        .Concat(this.solidWalls.Cast<Sprite>().Where(x => x is not null))
        .Concat(this.borderWalls.Cast<Sprite>())
        .Concat([this.fogOfWar])
        .Where(x => x is not null)!;

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        base.Update(gameTime);

        foreach (Sprite sprite in this.Sprites.ToList())
        {
            sprite.Update(gameTime);
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        base.Draw(gameTime);

        foreach (Sprite sprite in this.Sprites.ToList())
        {
            sprite.Draw(gameTime);
        }
    }

    /// <summary>
    /// Updates the fog of war.
    /// </summary>
    /// <param name="visibilityGrid">The visibility grid.</param>
    /// <param name="color">The color of the fog of war.</param>
    public void UpdateFogOfWar(bool[,] visibilityGrid, Color color)
    {
        if (this.fogOfWar is null)
        {
            this.fogOfWar = new Sprites.FogOfWar(visibilityGrid, this, color);
        }
        else
        {
            this.fogOfWar.VisibilityGrid = visibilityGrid;
        }
    }

    /// <summary>
    /// Resets the fog of war.
    /// </summary>
    public void ResetFogOfWar()
    {
        this.fogOfWar = null;
    }

    private void Logic_DimensionsChanged(object? sender, EventArgs args)
    {
        this.UpdateDrawData();

        this.solidWalls = new Sprites.Wall.Solid?[this.Logic.Dim, this.Logic.Dim];

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

    private void Logic_StateDeserialized(object? sender, EventArgs args)
    {
        this.SyncWalls();
        this.SyncTanks();
        this.SyncBullets();
        this.SyncZones();
    }

    private void SyncWalls()
    {
        for (int i = 0; i < this.Logic.WallGrid.GetLength(0); i++)
        {
            for (int j = 0; j < this.Logic.WallGrid.GetLength(1); j++)
            {
                var newWallLogic = this.Logic.WallGrid[i, j];
                if (newWallLogic is null)
                {
                    this.solidWalls[i, j] = null;
                }
                else if (this.solidWalls[i, j] is null)
                {
                    var wall = new Sprites.Wall.Solid(newWallLogic, this);
                    this.solidWalls[i, j] = wall;
                }
                else
                {
                    this.solidWalls[i, j]?.UpdateLogic(newWallLogic);
                }
            }
        }
    }

    private void SyncTanks()
    {
        foreach (var tank in this.Logic.Tanks)
        {
            var tankSprite = this.tanks.FirstOrDefault(t => t.Logic.Equals(tank));
            if (tankSprite == null)
            {
                tankSprite = new Sprites.Tank(tank, this);
                this.tanks.Add(tankSprite);
            }
            else
            {
                tankSprite.UpdateLogic(tank);
            }
        }

        _ = this.tanks.RemoveAll(t => !this.Logic.Tanks.Any(t2 => t2.Equals(t.Logic)));
    }

    private void SyncBullets()
    {
        foreach (var bullet in this.Logic.Bullets)
        {
            var bulletSprite = this.bullets.FirstOrDefault(b => b.Logic.Equals(bullet));
            if (bulletSprite == null)
            {
                bulletSprite = new Sprites.Bullet(bullet, this);
                this.bullets.Add(bulletSprite);
            }
            else
            {
                bulletSprite.UpdateLogic(bullet);
            }
        }

        _ = this.bullets.RemoveAll(b => !this.Logic.Bullets.Any(b2 => b2.Equals(b.Logic)));
    }

    private void SyncZones()
    {
        foreach (var zone in this.Logic.Zones)
        {
            var zoneSprite = this.zones.FirstOrDefault(z => z.Logic.Equals(zone));
            if (zoneSprite == null)
            {
                zoneSprite = new Sprites.Zone(zone, this);
                this.zones.Add(zoneSprite);
            }
            else
            {
                zoneSprite.UpdateLogic(zone);
            }
        }

        _ = this.zones.RemoveAll(z => !this.Logic.Zones.Any(z2 => z2.Equals(z.Logic)));
    }

    private void UpdateDrawData()
    {
        if (this.Logic.Dim == 0)
        {
            return;
        }

        var gridDim = this.Logic.Dim;
        var size = this.Transform.Size.X;
        var tileSize = this.TileSize = size / gridDim;
        this.DrawOffset = (int)((size - (gridDim * tileSize) + tileSize) / 4f);
        this.DrawDataChanged?.Invoke(this, EventArgs.Empty);
    }
}
