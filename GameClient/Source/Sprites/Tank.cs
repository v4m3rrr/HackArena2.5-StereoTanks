using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a tank sprite.
/// </summary>
internal class Tank : ISprite, IDetectableByRadar
{
#if STEREO
    private static readonly ScalableTexture2D.Static StaticTankLightTexture = new("Images/Game/tank_light.svg");
    private static readonly ScalableTexture2D.Static StaticTankHeavyTexture = new("Images/Game/tank_heavy.svg");
#else
    private static readonly ScalableTexture2D.Static StaticTankTexture = new("Images/Game/tank.svg");
#endif

    private static readonly ScalableTexture2D.Static StaticTurretTexture = new("Images/Game/turret.svg");

    private readonly GridComponent grid;
    private readonly ScalableTexture2D turretTexture;

    private readonly ScalableTexture2D tankTexture;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// </summary>
    /// <param name="logic">The tank logic.</param>
    /// <param name="grid">The grid component.</param>
    public Tank(GameLogic.Tank logic, GridComponent grid)
    {
        this.Logic = logic;
        this.grid = grid;

#if STEREO
        var staticTankTexture = logic.Type switch
        {
            TankType.Light => StaticTankLightTexture,
            TankType.Heavy => StaticTankHeavyTexture,
            _ => throw new ArgumentOutOfRangeException(nameof(logic), $"Invalid tank type: {logic.Type}"),
        };
#else
        var staticTankTexture = StaticTankTexture;
#endif

        this.tankTexture = new ScalableTexture2D(staticTankTexture)
        {
            Color = new Color(logic.Owner.Color),
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize, grid.TileSize),
            },
        };

        this.turretTexture = new ScalableTexture2D(StaticTurretTexture)
        {
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize, grid.TileSize),
            },
        };
    }

    /// <summary>
    /// Gets the tank logic.
    /// </summary>
    public GameLogic.Tank Logic { get; private set; }

    /// <inheritdoc/>
    float IDetectableByRadar.Opacity
    {
        get => this.tankTexture.Opacity;
        set => this.tankTexture.Opacity = this.turretTexture.Opacity = value;
    }

    /// <inheritdoc/>
    public static void LoadContent()
    {
#if STEREO
        StaticTankLightTexture.Load();
        StaticTankHeavyTexture.Load();
#else
        StaticTankTexture.Load();
#endif
        StaticTurretTexture.Load();
    }

    /// <inheritdoc/>
    IDetectableByRadar IDetectableByRadar.Copy()
    {
        return new Tank(this.Logic, this.grid);
    }

    /// <summary>
    /// Updates the tank logic.
    /// </summary>
    /// <param name="logic">The new tank logic.</param>
    public void UpdateLogic(GameLogic.Tank logic)
    {
        this.Logic = logic;
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
    {
        if (this.Logic.IsDead)
        {
            return;
        }

        this.tankTexture.Rotation = DirectionUtils.ToRadians(this.Logic.Direction);
        this.turretTexture.Rotation = DirectionUtils.ToRadians(this.Logic.Turret.Direction);

        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        this.tankTexture.Transform.Location = this.turretTexture.Transform.Location
            = new Point(
                gridLeft + (this.Logic.X * tileSize) + drawOffset,
                gridTop + (this.Logic.Y * tileSize) + drawOffset);

        this.tankTexture.Transform.Size = this.turretTexture.Transform.Size
            = new Point(tileSize, tileSize);

#if STEREO
        StaticTankLightTexture.Transform.Size = new Point(this.grid.TileSize);
        StaticTankHeavyTexture.Transform.Size = new Point(this.grid.TileSize);
#else
        StaticTankTexture.Transform.Size = new Point(this.grid.TileSize);
#endif
        StaticTurretTexture.Transform.Size = new Point(this.grid.TileSize);

        this.tankTexture.Update(gameTime);
        this.turretTexture.Update(gameTime);
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
        if (this.Logic.IsDead)
        {
            return;
        }

        this.tankTexture.Draw(gameTime);
        this.turretTexture.Draw(gameTime);
    }
}
