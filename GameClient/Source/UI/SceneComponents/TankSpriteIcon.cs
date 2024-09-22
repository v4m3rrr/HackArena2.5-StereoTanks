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

    /// <summary>
    /// Initializes a new instance of the <see cref="TankSpriteIcon"/> class.
    /// </summary>
    public TankSpriteIcon()
    {
        this.tankTexture = new ScalableTexture2D("Images/Game/tank.svg")
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
    public void SetColor(Color color)
    {
        this.tankTexture.Color = color;
    }
}
