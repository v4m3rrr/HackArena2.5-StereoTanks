using System;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

/// <summary>
/// Represents a player's health displayed on a player bar.
/// </summary>
internal class HealthBar : PlayerBarComponent
{
    private const float RelativeHeight = 0.5f;
    private readonly SolidColor bar;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthBar"/> class.
    /// </summary>
    /// <param name="player">The player whose health will be displayed.</param>
    public HealthBar(Player player)
        : base(player)
    {
        this.bar = new RoundedSolidColor(new Color(player.Color), 14)
        {
            Parent = this,
            Transform =
            {
                RelativeSize = new Vector2(1f, RelativeHeight),
                Alignment = Alignment.Left,
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

        this.bar.IsEnabled = true;

        if (this.Player.RegenProgress is not null)
        {
            this.bar.Color = new Color(this.bar.Color, 100);
            var progress = Math.Max(float.Epsilon, this.Player.RegenProgress ?? 0f);
            this.bar.Transform.RelativeSize = new Vector2(progress, RelativeHeight);
            return;
        }

        var hp = this.Player.Tank?.Health;

        if (hp is null)
        {
            this.bar.IsEnabled = false;
            return;
        }

        this.bar.Color = new Color(this.bar.Color, 255);
        this.bar.Transform.RelativeSize = new Vector2(hp.Value / 100f, RelativeHeight);

        base.Update(gameTime);
    }
}
