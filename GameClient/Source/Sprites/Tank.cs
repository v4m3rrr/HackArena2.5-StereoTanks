using GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a tank sprite.
/// </summary>
internal class Tank : Sprite
{
    private static readonly Texture2D TankTexture;
    private static readonly Texture2D TankFillTexture;
    private static readonly Texture2D TurretTexture;

    private readonly GridComponent grid;

    static Tank()
    {
        TankTexture = ContentController.Content.Load<Texture2D>("Images/Tank");
        TankFillTexture = ContentController.Content.Load<Texture2D>("Images/TankFill");
        TurretTexture = ContentController.Content.Load<Texture2D>("Images/TankTurret");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// </summary>
    /// <param name="logic">The tank logic.</param>
    /// <param name="grid">The grid component.</param>
    public Tank(GameLogic.Tank logic, GridComponent grid)
    {
        this.Logic = logic;
        this.grid = grid;
    }

    /// <summary>
    /// Gets the tank logic.
    /// </summary>
    public GameLogic.Tank Logic { get; private set; }

    /// <summary>
    /// Updates the tank logic.
    /// </summary>
    /// <param name="logic">The new tank logic.</param>
    public void UpdateLogic(GameLogic.Tank logic)
    {
        this.Logic = logic;
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

        float tankRotation = DirectionUtils.ToRotation(this.Logic.Direction);
        float turretRotation = DirectionUtils.ToRotation(this.Logic.Turret.Direction);

        var rect = new Rectangle(
            gridLeft + (this.Logic.X * tileSize) + drawOffset,
            gridTop + (this.Logic.Y * tileSize) + drawOffset,
            tileSize,
            tileSize);

        SpriteBatchController.SpriteBatch.Draw(
            TankTexture,
            rect,
            null,
            Color.White,
            tankRotation,
            TankTexture.Bounds.Size.ToVector2() / 2f,
            SpriteEffects.None,
            1.0f);

        SpriteBatchController.SpriteBatch.Draw(
            TankFillTexture,
            rect,
            null,
            new Color(this.Logic.Color),
            tankRotation,
            TankTexture.Bounds.Size.ToVector2() / 2f,
            SpriteEffects.None,
            1.0f);

        SpriteBatchController.SpriteBatch.Draw(
            TurretTexture,
            rect,
            null,
            new Color(this.Logic.Color),
            turretRotation,
            TankTexture.Bounds.Size.ToVector2() / 2f,
            SpriteEffects.None,
            1.0f);
    }
}
