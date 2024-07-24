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
    private readonly Sprites.Wall?[,] walls = new Sprites.Wall[Grid.Dim, Grid.Dim];
    private readonly List<Sprites.Tank> tanks = new();
    private readonly List<Sprites.Bullet> bullets = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GridComponent"/> class.
    /// </summary>
    /// <param name="logic">The grid logic.</param>
    public GridComponent(Grid logic)
    {
        this.Logic = logic;
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

    private IEnumerable<Sprite> Sprites => this.tanks.Cast<Sprite>()
        .Concat(this.walls.Cast<Sprite>().Where(x => x is not null))
        .Concat(this.bullets.Cast<Sprite>());

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        base.Update(gameTime);

        foreach (Sprite sprite in this.Sprites)
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

        foreach (Sprite sprite in this.Sprites)
        {
            sprite.Draw(gameTime);
        }
    }

    private void Logic_StateDeserialized(object? sender, EventArgs args)
    {
        this.SyncWalls();
        this.SyncTanks();
        this.SyncBullets();
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
                    this.walls[i, j] = null;
                }
                else if (this.walls[i, j] is null)
                {
                    var wall = new Sprites.Wall(newWallLogic, this);
                    this.walls[i, j] = wall;
                }
                else
                {
                    this.walls[i, j]?.UpdateLogic(newWallLogic);
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

    private void UpdateDrawData()
    {
        this.TileSize = this.Transform.Size.X / Grid.Dim;
        this.DrawOffset = (int)(((this.Transform.Size.X - (Grid.Dim * this.TileSize)) / 2f) + (this.TileSize / 2f));
        this.DrawDataChanged?.Invoke(this, EventArgs.Empty);
    }
}
