using System;
using GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.PlayerBarComponents;

/// <summary>
/// Represents the bullet count of a player displayed on a player bar.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BulletCount"/> class.
/// </remarks>
/// <param name="player">The player whose bullet count will be displayed.</param>
internal class BulletCount(Player player) : PlayerBarComponent(player)
{
    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled || this.Player.Tank.IsDead)
        {
            return;
        }

        var bulletTexture = Sprites.Bullet.Texture;
        var centerRect = this.Transform.DestRectangle;
        var bulletCount = this.Player.Tank.Turret.BulletCount;
        var bulletRegenProgress = this.Player.Tank.Turret.BulletRegenProgress;
        var maxBulletCount = TankTurret.MaxBulletCount;

        var spriteBatch = SpriteBatchController.SpriteBatch;
        var textureWidth = bulletTexture.Width;
        var textureHeight = bulletTexture.Height;

        for (int i = 0; i < maxBulletCount; i++)
        {
            var destRect = new Rectangle(
                centerRect.Left + (i * centerRect.Height),
                centerRect.Top,
                centerRect.Height,
                centerRect.Height);

            var color = Color.White * (i < bulletCount ? 1.0f : 0.3f);

            spriteBatch.Draw(
                bulletTexture,
                destRect,
                null,
                color,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                1.0f);

            if (i == bulletCount)
            {
                var regenHeight = (int)Math.Ceiling(textureHeight * bulletRegenProgress);
                var regenYOffset = (int)Math.Ceiling(destRect.Height * (1 - bulletRegenProgress));

                var sourceRect = new Rectangle(0, textureHeight - regenHeight, textureWidth, regenHeight);
                destRect.Y += regenYOffset;
                destRect.Height = (int)(centerRect.Height * bulletRegenProgress);

                spriteBatch.Draw(
                    bulletTexture,
                    destRect,
                    sourceRect,
                    Color.White * 0.8f,
                    0.0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    1.0f);
            }
        }

        base.Draw(gameTime);
    }
}
