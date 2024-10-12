using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a laser sprite.
/// </summary>
/// <param name="logic">The logic of the laser.</param>
/// <param name="grid">The grid component.</param>
internal class Laser(GameLogic.Laser logic, GridComponent grid) : Sprite, IDetectableByRadar
{
    private const float OffsetFactor1 = 0.45f;
    private const float OffsetFactor2 = 0.40f;
    private const float LaserThickness1 = 0.1f;
    private const float LaserThickness2 = 0.2f;

    private Rectangle innerDestRectangle;
    private Rectangle outerDestRectangle;
    private float opacity = 1f;

    /// <summary>
    /// Gets the logic of the laser.
    /// </summary>
    public GameLogic.Laser Logic { get; private set; } = logic;

    /// <inheritdoc/>
    float IDetectableByRadar.Opacity
    {
        get => this.opacity;
        set => this.opacity = value;
    }

    /// <summary>
    /// Updates the logic of the laser.
    /// </summary>
    /// <param name="logic">The new logic of the laser.</param>
    public void UpdateLogic(GameLogic.Laser logic)
    {
        this.Logic = logic;
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        int tileSize = grid.TileSize;
        int drawOffset = grid.DrawOffset;
        int gridLeft = grid.Transform.DestRectangle.Left;
        int gridTop = grid.Transform.DestRectangle.Top;

        var leftTopCornerOfTile = new Point(
            gridLeft + (this.Logic.X * tileSize) + drawOffset,
            gridTop + (this.Logic.Y * tileSize) + drawOffset);

        this.innerDestRectangle = this.outerDestRectangle = new Rectangle(
            leftTopCornerOfTile,
            new Point(tileSize));

        if (this.Logic.Orientation is GameLogic.Orientation.Horizontal)
        {
            this.innerDestRectangle.Y += (int)(tileSize * OffsetFactor1);
            this.outerDestRectangle.Y += (int)(tileSize * OffsetFactor2);
            this.innerDestRectangle.Height = (int)(tileSize * LaserThickness1);
            this.outerDestRectangle.Height = (int)(tileSize * LaserThickness2);
        }
        else if (this.Logic.Orientation is GameLogic.Orientation.Vertical)
        {
            this.innerDestRectangle.X += (int)(tileSize * OffsetFactor1);
            this.outerDestRectangle.X += (int)(tileSize * OffsetFactor2);
            this.innerDestRectangle.Width = (int)(tileSize * LaserThickness1);
            this.outerDestRectangle.Width = (int)(tileSize * LaserThickness2);
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = SpriteBatchController.SpriteBatch;

        spriteBatch.Draw(
            SpriteBatchController.WhitePixel,
            this.outerDestRectangle,
            null,
            Color.White * 0.75f * this.opacity,
            0f,
            Vector2.Zero,
            SpriteEffects.None,
            0f);

        spriteBatch.Draw(
            SpriteBatchController.WhitePixel,
            this.innerDestRectangle,
            null,
            Color.White * this.opacity,
            0f,
            Vector2.Zero,
            SpriteEffects.None,
            0f);
    }
}
