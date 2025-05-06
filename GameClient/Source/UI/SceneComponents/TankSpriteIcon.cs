using GameClient;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI.SceneComponents;

/// <summary>
/// Represents a player's tank sprite displayed on a player slot panel.
/// </summary>
internal class TankSpriteIcon : Component
{
    private readonly ScalableTexture2D turretTexture;
    private readonly ScalableTexture2D tankTexture;

#if STEREO
    /// <summary>
    /// Initializes a new instance of the <see cref="TankSpriteIcon"/> class.
    /// </summary>
    /// <param name="type">The type of tank to display.</param>
    public TankSpriteIcon(TankType type)
    {
        var tankAssetPath = type switch
        {
            TankType.Light => "Images/Game/tank_light.svg",
            TankType.Heavy => "Images/Game/tank_heavy.svg",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
#else
    /// <summary>
    /// Initializes a new instance of the <see cref="TankSpriteIcon"/> class.
    /// </summary>
    public TankSpriteIcon()
    {
        var tankAssetPath = "Images/Game/tank.svg";
#endif

        this.tankTexture = new ScalableTexture2D(tankAssetPath)
        {
            Parent = this,
            Transform =
            {
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };

        this.turretTexture = new ScalableTexture2D("Images/Game/turret.svg")
        {
            Parent = this,
            Transform =
            {
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };
    }

    /// <summary>
    /// Sets the tank sprite color.
    /// </summary>
    /// <param name="color">The color to set.</param>
    /// <remarks>
    /// The color does not affect the turret sprite, which is always white.
    /// </remarks>
    public void SetColor(Color color)
    {
        this.tankTexture.Color = color;
    }

    /// <summary>
    /// Sets the opacity of the tank sprite.
    /// </summary>
    /// <param name="opacity">The opacity to set (between 0.0f and 1.0f).</param>
    /// <remarks>
    /// The opacity is applied to both the tank and turret sprites.
    /// </remarks>
    public void SetOpacity(float opacity)
    {
        this.tankTexture.Opacity = opacity;
        this.turretTexture.Opacity = opacity;
    }
}
