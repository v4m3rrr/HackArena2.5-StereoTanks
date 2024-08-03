using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;
using System;

namespace GameClient.PlayerBarComponents;

/// <summary>
/// Represents a player's health displayed on a player bar.
/// </summary>
internal class HealthBar : PlayerBarComponent
{
    private readonly SolidColor bar;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthBar"/> class.
    /// </summary>
    /// <param name="player">The player whose health will be displayed.</param>
    public HealthBar(Player player)
        : base(player)
    {
        this.bar = new SolidColor(new Color(player.Color)) { Parent = this };
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.bar.IsEnabled = true;

        if (this.Player.Tank?.RegenProgress is not null)
        {
            this.bar.Color = new Color(this.bar.Color, 100);
            var progress = Math.Max(float.Epsilon, this.Player.Tank.RegenProgress ?? 0f);
            this.bar.Transform.RelativeSize = new Vector2(progress, 1f);
            return;
        }

        var hp = this.Player.Tank?.Health;

        if (hp is null)
        {
            this.bar.IsEnabled = false;
            return;
        }


        this.bar.Color = new Color(this.bar.Color, 255);
        this.bar.Transform.RelativeSize = new Vector2(hp.Value / 100f, 1f);

        base.Update(gameTime);
    }
}