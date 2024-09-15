using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a fog of war sprite.
/// </summary>
/// <param name="visibilityGrid">The visibility grid.</param>
/// <param name="grid">The grid component.</param>
/// <param name="color">The color of fog.</param>
internal class FogOfWar(bool[,] visibilityGrid, GridComponent grid, Color color) : Sprite
{
    private static readonly ScalableTexture2D.Static Texture;

    private readonly GridComponent grid = grid;
    private readonly List<Rectangle> destinationRects = [];
    private readonly Vector2 origin = Vector2.One / 2f;

    static FogOfWar()
    {
        Texture = new ScalableTexture2D.Static("Images/Game/fog_of_war.svg");
    }

    /// <summary>
    /// Gets or sets the visibility grid.
    /// </summary>
    public bool[,] VisibilityGrid { get; set; } = visibilityGrid;

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        Texture.Transform.Size = new Point(tileSize);

        this.destinationRects.Clear();

        for (int y = 0; y < this.VisibilityGrid.GetLength(1); y++)
        {
            for (int x = 0; x < this.VisibilityGrid.GetLength(0); x++)
            {
                if (this.VisibilityGrid[x, y])
                {
                    var rect = new Rectangle(
                        gridLeft + (x * tileSize) + drawOffset,
                        gridTop + (y * tileSize) + drawOffset,
                        tileSize,
                        tileSize);
                    this.destinationRects.Add(rect);
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = SpriteBatchController.SpriteBatch;

        foreach (Rectangle rect in this.destinationRects)
        {
            spriteBatch.Draw(
                Texture.Texture,
                rect,
                null,
                color * 0.8f,
                0f,
                this.origin,
                SpriteEffects.None,
                0f);
        }
    }
}
