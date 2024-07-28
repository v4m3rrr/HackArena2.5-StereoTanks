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

        var hp = this.Player.Tank.Health;
        if (hp <= 0)
        {
            this.bar.Color = new Color(this.bar.Color, 100);
            var progress = Math.Max(float.Epsilon, this.Player.Tank.RegenProgress);
            this.bar.Transform.RelativeSize = new Vector2(progress, 1f);
        }
        else
        {
            this.bar.Color = new Color(this.bar.Color, 255);
            this.bar.Transform.RelativeSize = new Vector2(hp / 100f, 1f);
        }

        base.Update(gameTime);
    }
}