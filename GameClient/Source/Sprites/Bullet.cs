using System;
using System.Collections.Generic;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a bullet sprite.
/// </summary>
internal class Bullet : Sprite, IDetectableByRadar
{
    private static readonly ScalableTexture2D.Static StaticTexture;
    private static float heightPercentage;

    private readonly ScalableTexture2D texture;
    private readonly GridComponent grid;

    private Vector2 position;
    private bool skipNextPositionUpdate;
    private bool isOutOfBounds;
    private Collision? collision;

    static Bullet()
    {
        StaticTexture = new ScalableTexture2D.Static("Images/Game/bullet_ts.svg");

        MonoTanks.InvokeOnMainThread(() =>
        {
            StaticTexture.Load();

            var data = new Color[StaticTexture.Texture.Width * StaticTexture.Texture.Height];
            StaticTexture.Texture.GetData(data);

            int firstHeight = -1;
            int lastHeight = -1;

            for (int i = 0; i < StaticTexture.Texture.Height; i++)
            {
                for (int j = 0; j < StaticTexture.Texture.Width; j++)
                {
                    if (data[(i * StaticTexture.Texture.Width) + j].A > 0)
                    {
                        if (firstHeight == -1)
                        {
                            firstHeight = i;
                        }

                        lastHeight = i;
                    }
                }
            }

            heightPercentage = (float)(lastHeight - firstHeight) / StaticTexture.Texture.Height;
        });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="logic">The bullet logic.</param>
    /// <param name="grid">The grid component.</param>
    public Bullet(GameLogic.Bullet logic, GridComponent grid)
        : this(logic, grid, StaticTexture)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="logic">The bullet logic.</param>
    /// <param name="grid">The grid component.</param>
    /// <param name="staticTexture">
    /// The static texture that will be used
    /// to create a new scalable texture for the bullet.
    /// </param>
    protected Bullet(GameLogic.Bullet logic, GridComponent grid, ScalableTexture2D.Static staticTexture)
    {
        this.texture = new ScalableTexture2D(staticTexture)
        {
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize),
            },
        };

        this.texture.Load();

        this.grid = grid;
        this.UpdateLogic(logic);
        this.position = new Vector2(this.Logic.X, this.Logic.Y);
        var (nx, ny) = DirectionUtils.Normal(this.Logic.Direction);
        this.position -= new Vector2(heightPercentage) * new Vector2(nx, ny);
        this.skipNextPositionUpdate = true;
    }

    /// <summary>
    /// Gets the bullet logic.
    /// </summary>
    public GameLogic.Bullet Logic { get; private set; } = default!;

    /// <inheritdoc/>
    float IDetectableByRadar.Opacity
    {
        get => this.texture.Opacity;
        set => this.texture.Opacity = value;
    }

    private bool IsCollisionDetected => this.collision is not null;

    /// <summary>
    /// Updates the bullet logic.
    /// </summary>
    /// <param name="logic">The new bullet logic.</param>
    public void UpdateLogic(GameLogic.Bullet logic)
    {
        this.Logic = logic;

        // If the logic is updated, the bullet on server does not collide.
        this.collision = null;

        // If the bullet is not in the correct position, update it.
        if (Math.Abs(this.position.X - this.Logic.X) > 1 || Math.Abs(this.position.Y - this.Logic.Y) > 1)
        {
            this.position = new Vector2(this.Logic.X, this.Logic.Y);
            var (nx, ny) = DirectionUtils.Normal(this.Logic.Direction);
            this.position -= new Vector2(heightPercentage) * new Vector2(nx, ny);
            this.skipNextPositionUpdate = true;
        }
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        this.UpdatePosition(gameTime);
        this.CheckCollision();

        if (this.IsCollisionDetected)
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
        var size = new Point(tileSize);

        StaticTexture.Transform.Size = size;

        this.texture.Transform.Location = location;
        this.texture.Transform.Size = size;
        this.texture.Rotation = DirectionUtils.ToRadians(this.Logic.Direction);

        this.texture.Update(gameTime);

        this.isOutOfBounds = this.position.X <= -0.5f
            || this.position.Y <= -0.5f
            || this.position.X >= this.grid.Logic.Dim
            || this.position.Y >= this.grid.Logic.Dim;
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (this.IsCollisionDetected || this.isOutOfBounds)
        {
            return;
        }

        this.texture.Draw(gameTime);
    }

    private void UpdatePosition(GameTime gameTime)
    {
        if (this.IsCollisionDetected)
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

        var posX = Math.Clamp((int)(this.position.X + 0.5f), -1, gridDim);
        var posY = Math.Clamp((int)(this.position.Y + 0.5f), -1, gridDim);
        var trajectories = new Dictionary<GameLogic.Bullet, List<(int X, int Y)>>
        {
            [this.Logic] = [(posX, posY)],
        };

        this.collision = CollisionDetector.CheckBulletCollision(this.Logic, this.grid.Logic, trajectories);
    }
}
