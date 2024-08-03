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

    private Rectangle destinationRect;
    private float tankRotation;
    private float turretRotation;

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
        if (this.Logic.IsDead)
        {
            return;
        }

        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        this.tankRotation = DirectionUtils.ToRadians(this.Logic.Direction);
        this.turretRotation = DirectionUtils.ToRadians(this.Logic.Turret.Direction);

        this.destinationRect = new Rectangle(
            gridLeft + (this.Logic.X * tileSize) + drawOffset,
            gridTop + (this.Logic.Y * tileSize) + drawOffset,
            tileSize,
            tileSize);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (this.Logic.IsDead)
        {
            return;
        }

        SpriteBatchController.SpriteBatch.Draw(
            TankTexture,
            this.destinationRect,
            null,
            Color.White,
            this.tankRotation,
            TankTexture.Bounds.Size.ToVector2() / 2f,
            SpriteEffects.None,
            1.0f);

        SpriteBatchController.SpriteBatch.Draw(
            TankFillTexture,
            this.destinationRect,
            null,
            new Color(this.Logic.Owner.Color),
            this.tankRotation,
            TankTexture.Bounds.Size.ToVector2() / 2f,
            SpriteEffects.None,
            1.0f);

        SpriteBatchController.SpriteBatch.Draw(
            TurretTexture,
            this.destinationRect,
            null,
            Color.White,
            this.turretRotation,
            TankTexture.Bounds.Size.ToVector2() / 2f,
            SpriteEffects.None,
            1.0f);
    }
}
