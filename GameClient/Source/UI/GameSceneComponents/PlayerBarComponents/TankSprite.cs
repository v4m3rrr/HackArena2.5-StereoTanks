using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

/// <summary>
/// Represents a player's tank sprite displayed on a player bar.
/// </summary>
internal class TankSprite : PlayerBarComponent
{
    private readonly ScalableTexture2D tankTexture;
    private readonly ScalableTexture2D turretTexture;

    /// <summary>
    /// Initializes a new instance of the <see cref="TankSprite"/> class.
    /// </summary>
    /// <param name="player">The player whose tank sprite will be displayed.</param>
    public TankSprite(Player player)
        : base(player)
    {
        this.tankTexture = new ScalableTexture2D("Images/Game/tank.svg")
        {
            Parent = this,
            Color = new Color(player.Color),
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };

        this.turretTexture = new ScalableTexture2D("Images/Game/turret.svg")
        {
            Parent = this,
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        if (this.Player.Tank is not null)
        {
            var tankDirection = this.Player.Tank.Direction;
            var tankRotation = DirectionUtils.ToRadians(tankDirection);
            this.tankTexture.Rotation = tankRotation;

            var turretDirection = this.Player.Tank.Turret.Direction;
            var turretRotation = DirectionUtils.ToRadians(turretDirection);
            this.turretTexture.Rotation = turretRotation;
        }

        base.Update(gameTime);
    }
}
