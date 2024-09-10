using System;
using System.Collections.Generic;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a bullet sprite.
/// </summary>
internal class Bullet : Sprite
{
    /// <summary>
    /// Gets the static bullet texture.
    /// </summary>
    public static readonly ScalableTexture2D.Static StaticTexture;

    private readonly ScalableTexture2D texture;
    private readonly GridComponent grid;

    private Vector2 position;

    private bool isCollisionDetected;
    private bool skipNextPositionUpdate;
    private bool isOutOfBounds;

    static Bullet()
    {
        StaticTexture = new ScalableTexture2D.Static("Images/Game/bullet.svg");
        StaticTexture.Load();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="logic">The bullet logic.</param>
    /// <param name="grid">The grid component.</param>
    public Bullet(GameLogic.Bullet logic, GridComponent grid)
    {
        this.texture = new ScalableTexture2D(StaticTexture)
        {
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize / 8, (int)(grid.TileSize / 2.5f)),
            },
        };

        this.grid = grid;
        this.UpdateLogic(logic);
        this.position = new Vector2(this.Logic.X + 0.5f, this.Logic.Y + 0.5f);
        this.skipNextPositionUpdate = true;
    }

    /// <summary>
    /// Gets the bullet logic.
    /// </summary>
    public GameLogic.Bullet Logic { get; private set; } = default!;

    /// <summary>
    /// Updates the bullet logic.
    /// </summary>
    /// <param name="logic">The new bullet logic.</param>
    public void UpdateLogic(GameLogic.Bullet logic)
    {
        this.Logic = logic;

        // If the logic is updated, the bullet on server does not collide.
        this.isCollisionDetected = false;

        // If the bullet is not in the correct position, update it.
        if (Math.Abs(this.position.X - this.Logic.X) > 1 || Math.Abs(this.position.Y - this.Logic.Y) > 1)
        {
            this.position = new Vector2(this.Logic.X + 0.5f, this.Logic.Y + 0.5f);
            this.skipNextPositionUpdate = true;
        }
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        this.UpdatePosition(gameTime);
        this.CheckCollision();

        if (this.isCollisionDetected)
        {
            return;
        }

        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        var location = new Point(
            gridLeft + ((int)(this.position.X * tileSize)) + drawOffset,
            gridTop + ((int)(this.position.Y * tileSize)) + drawOffset);
        var size = new Point(tileSize / 8, (int)(tileSize / 2.5f));

        this.texture.Transform.Location = location;
        this.texture.Transform.Size = size;
        this.texture.Rotation = DirectionUtils.ToRadians(this.Logic.Direction);

        this.isOutOfBounds = this.position.X < 0
            || this.position.Y < 0
            || this.position.X >= this.grid.Logic.Dim
            || this.position.Y >= this.grid.Logic.Dim;
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (this.isCollisionDetected || this.isOutOfBounds)
        {
            return;
        }

        this.texture.Draw(gameTime);
    }

    private void UpdatePosition(GameTime gameTime)
    {
        if (this.isCollisionDetected)
        {
            return;
        }

        if (this.skipNextPositionUpdate)
        {
            this.skipNextPositionUpdate = false;
            return;
        }

        var (nx, ny) = DirectionUtils.Normal(this.Logic.Direction);
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var factor = deltaTime * this.Logic.Speed / Scenes.Game.ServerBroadcastInterval * 1000f;
        this.position += new Vector2(nx, ny) * factor;
    }

    private void CheckCollision()
    {
        // TODO: Add other bullets trajectories.
        // Currently, only collision with walls and tanks is checked.
        // A dictionary below is created to avoid errors in the CollisionDetector.
        var gridDim = this.grid.Logic.Dim;
        var posX = Math.Clamp((int)this.position.X, 0, gridDim - 1);
        var posY = Math.Clamp((int)this.position.Y, 0, gridDim - 1);
        var trajectories = new Dictionary<GameLogic.Bullet, List<(int X, int Y)>>
        {
            [this.Logic] = [(posX, posY)],
        };

        ICollision? collision = CollisionDetector.CheckBulletCollision(this.Logic, this.grid.Logic, trajectories);
        if (collision is not null)
        {
            this.isCollisionDetected = true;
        }
    }
}
