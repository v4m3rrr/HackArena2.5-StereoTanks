using GameClient.UI;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a radar effect sprite.
/// </summary>
internal class RadarEffect : ISprite
{
    private const int EffectDuration = 1700;
    private const int DetectionDuration = 3500;
    private const int Thickness = 4;

    private readonly IEnumerable<IDetectableByRadar> detectedSprites;
    private readonly GridComponent grid;
    private readonly Circle circle;

    private int effectRemainingTime = EffectDuration;
    private int detectionRemainingTime = DetectionDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="RadarEffect"/> class.
    /// </summary>
    /// <param name="tank">The tank that activated the radar effect.</param>
    /// <param name="grid">The grid component where the radar effect is displayed.</param>
    /// <param name="sprites">The sprites on the grid.</param>
    public RadarEffect(GameLogic.Tank tank, GridComponent grid, IEnumerable<ISprite> sprites)
    {
        this.detectedSprites = Scenes.Game.PlayerId is not null
            ? []
            : GetDetectedSprites(tank, sprites);

        this.grid = grid;
        this.Tank = tank;

        this.circle = new Circle(Thickness)
        {
            Color = new Color(tank.Owner.Color),
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize),
            },
        };

        this.circle.Load();

        this.UpdateEffect(new GameTime());
    }

    /// <summary>
    /// Gets the tank that activated the radar effect.
    /// </summary>
    public GameLogic.Tank Tank { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the radar effect is expired.
    /// </summary>
    /// <remarks>
    /// The radar effect is expired when both the effect and detection times are over.
    /// </remarks>
    public bool IsExpired => this.effectRemainingTime <= 0
        && (this.detectionRemainingTime <= 0 || this.RadarAbility is null);

    private RadarAbility? RadarAbility => this.Tank.GetAbility<RadarAbility>();

    /// <summary>
    /// Updates the tank logic that activated the radar effect.
    /// </summary>
    /// <param name="tank">The tank logic that activated the radar effect.</param>
    public void UpdateTank(GameLogic.Tank tank)
    {
        this.Tank = tank;
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
    {
        if (this.Tank.IsDead)
        {
            return;
        }

        if (this.IsExpired)
        {
            this.circle.Texture.Dispose();
            return;
        }

        this.UpdateEffect(gameTime);

        foreach (IDetectableByRadar sprite in this.detectedSprites)
        {
            sprite.Update(gameTime);
        }

        this.UpdateDetection(gameTime);
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
        if (this.IsExpired || this.Tank.IsDead)
        {
            return;
        }

        foreach (IDetectableByRadar sprite in this.detectedSprites)
        {
            sprite.Draw(gameTime);
        }

        this.circle.Draw(gameTime);
    }

    private static List<IDetectableByRadar> GetDetectedSprites(GameLogic.Tank tank, IEnumerable<ISprite> sprites)
    {
        var detectedSprites = new List<IDetectableByRadar>();
        foreach (var sprite in sprites)
        {
            if (sprite is Tank spriteTank && spriteTank.Logic.Equals(tank))
            {
                continue;
            }

            if (sprite is IDetectableByRadar radarDetectable)
            {
                detectedSprites.Add(radarDetectable.Copy());
            }
        }

        return detectedSprites;
    }

    private void UpdateEffect(GameTime gameTime)
    {
        var effectProgress = 1f - ((float)this.effectRemainingTime / EffectDuration);

        var sizeFactor = 1.2f + (effectProgress * 3.3f);
        var newSize = (int)(this.grid.TileSize * sizeFactor);
        if (newSize % 2 == 1)
        {
            // It looks better if the size is an even number.
            newSize++;
        }

        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;
        var halfSize = newSize / 2;

        var xTileOffset = this.Tank.X * tileSize;
        var yTileOffset = this.Tank.Y * tileSize;
        var location = new Point(gridLeft + xTileOffset, gridTop + yTileOffset);
        location += new Point(drawOffset - halfSize + (tileSize / 2));

        this.circle.Transform.Location = location;
        this.circle.Transform.Size = new Point(newSize);
        this.circle.Opacity = Animations.EaseInOut(1f - effectProgress);

        this.effectRemainingTime -= gameTime.ElapsedGameTime.Milliseconds;
    }

    private void UpdateDetection(GameTime gameTime)
    {
        var detectionProgress = 1f - ((float)this.detectionRemainingTime / DetectionDuration);

        foreach (IDetectableByRadar sprite in this.detectedSprites)
        {
            sprite.Opacity = Animations.EaseInOut(1f - detectionProgress);
        }

        this.detectionRemainingTime -= gameTime.ElapsedGameTime.Milliseconds;
    }
}
