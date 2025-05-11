using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a fog of war sprite.
/// </summary>
/// <param name="tank">The tank logic associated with this fog of war.</param>
/// <param name="grid">The grid component.</param>
/// <param name="color">The color of the fog of war.</param>
internal class FogOfWar(GameLogic.Tank tank, GridComponent grid, Color color) : ISprite
{
    private static readonly ScalableTexture2D.Static Texture = new("Images/Game/fog_of_war.svg");

    private readonly GridComponent grid = grid;
    private readonly List<Rectangle> destinationRects = [];
    private readonly Vector2 origin = Vector2.One / 2f;

    /// <summary>
    /// Gets the tank logic associated with this fog of war.
    /// </summary>
    public GameLogic.Tank Tank { get; } = tank;

    /// <summary>
    /// Gets or sets the visibility grid.
    /// </summary>
    public bool[,] VisibilityGrid { get; set; } = tank.VisibilityGrid
        ?? throw new InvalidOperationException("The tank logic does not have a visibility grid.");

    /// <inheritdoc/>
    public static void LoadContent()
    {
        Texture.Load();
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
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
    public void Draw(GameTime gameTime)
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
