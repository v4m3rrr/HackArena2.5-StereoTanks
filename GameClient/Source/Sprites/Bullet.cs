using GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a bullet sprite.
/// </summary>
internal class Bullet : Sprite
{
    private static readonly Texture2D Texture;
    private readonly GridComponent grid;
    private Vector2 position;

    static Bullet()
    {
        Texture = ContentController.Content.Load<Texture2D>("Images/Bullet");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="logic">The bullet logic.</param>
    /// <param name="grid">The grid component.</param>
    public Bullet(GameLogic.Bullet logic, GridComponent grid)
    {
        this.grid = grid;
        this.UpdateLogic(logic);
    }

    /// <summary>
    /// Gets the bullet logic.
    /// </summary>
    public GameLogic.Bullet Logic { get; private set; } = default!;

    private Vector2 DirectionNormal => this.Logic.Direction switch
    {
        Direction.Up => new Vector2(0, -1),
        Direction.Down => new Vector2(0, 1),
        Direction.Left => new Vector2(-1, 0),
        Direction.Right => new Vector2(1, 0),
        _ => Vector2.Zero,
    };

    /// <summary>
    /// Updates the bullet logic.
    /// </summary>
    /// <param name="logic">The new bullet logic.</param>
    public void UpdateLogic(GameLogic.Bullet logic)
    {
        this.Logic = logic;
        this.position = new Vector2(this.Logic.X, this.Logic.Y);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        float rotation = DirectionUtils.ToRotation(this.Logic.Direction);

        var rect = new Rectangle(
            gridLeft + ((int)(this.position.X * tileSize)) + drawOffset,
            gridTop + ((int)(this.position.Y * tileSize)) + drawOffset,
            tileSize,
            tileSize);

        if (rect.Left < gridLeft || rect.Right > gridLeft + this.grid.Transform.DestRectangle.Width ||
                       rect.Top < gridTop || rect.Bottom > gridTop + this.grid.Transform.DestRectangle.Height)
        {
            return;
        }

        SpriteBatchController.SpriteBatch.Draw(
            Texture,
            rect,
            null,
            Color.White,
            rotation,
            Texture.Bounds.Size.ToVector2() / 2f,
            SpriteEffects.None,
            1.0f);
    }
}
