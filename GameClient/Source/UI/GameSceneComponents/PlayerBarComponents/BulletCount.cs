using System;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

/// <summary>
/// Represents the bullet count of a player displayed on a player bar.
/// </summary>
internal class BulletCount : PlayerBarComponent
{
    private static readonly ScalableTexture2D.Static StaticTexture;

    private readonly ScalableTexture2D[] textures;
    private readonly ScalableTexture2D[] backgroundTextures;
    private readonly ListBox listBox;

    static BulletCount()
    {
        StaticTexture = new ScalableTexture2D.Static("Images/Game/bullet.svg");
        StaticTexture.Load();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BulletCount"/> class.
    /// </summary>
    /// <param name="player">The player whose bullet count will be displayed.</param>
    public BulletCount(Player player)
        : base(player)
    {
        this.textures = new ScalableTexture2D[Turret.MaxBulletCount];
        this.backgroundTextures = new ScalableTexture2D[Turret.MaxBulletCount];

        this.listBox = new ListBox
        {
            Parent = this,
            Orientation = MonoRivUI.Orientation.Horizontal,
            Spacing = 5,
            Transform =
            {
                Alignment = Alignment.TopLeft,
            },
        };

        this.listBox.ComponentAdded += (s, e) =>
        {
            if (e is ScalableTexture2D texture)
            {
                StaticTexture.Transform.Size = texture.Transform.Size;
            }
        };

        for (int i = 0; i < this.textures.Length; i++)
        {
            this.backgroundTextures[i] = new ScalableTexture2D(StaticTexture)
            {
                Parent = this.listBox.ContentContainer,
                Color = Color.White * 0.3f,
            };

            this.textures[i] = new ScalableTexture2D(StaticTexture)
            {
                Parent = this.backgroundTextures[i],
                MatchDestinationToSource = true,
            };
        }

        this.textures[^1].Transform.SizeChanged += (s, e) =>
        {
            var component = (s as Transform)!.Component as ScalableTexture2D;
            StaticTexture.Transform.Size = component!.Transform.Size;
        };
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled || (this.Player.Tank?.IsDead ?? true))
        {
            return;
        }

        var bulletCount = this.Player.Tank.Turret.BulletCount;
        var bulletRegenProgress = this.Player.Tank.Turret.BulletRegenProgress;

        for (int i = 0; i < this.textures.Length; i++)
        {
            if (i < bulletCount)
            {
                this.EnableTexture(i, null, Color.White);
            }
            else if (bulletRegenProgress is not null && i == bulletCount)
            {
                var sourceRect = CalculateBulletRegenSourceRect(bulletRegenProgress.Value);
                this.EnableTexture(i, sourceRect, Color.White * 0.75f);
            }
            else
            {
                this.DisableTexture(i);
            }
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled || (this.Player.Tank?.IsDead ?? true))
        {
            return;
        }

        base.Draw(gameTime);
    }

    private static Rectangle CalculateBulletRegenSourceRect(float bulletRegenProgress)
    {
        int height = StaticTexture.Texture.Height;
        int regenHeight = (int)Math.Ceiling((1 - bulletRegenProgress) * height);
        return new Rectangle(0, regenHeight, StaticTexture.Texture.Width, (int)(bulletRegenProgress * height));
    }

    private void EnableTexture(int index, Rectangle? sourceRect, Color color)
    {
        this.textures[index].IsEnabled = true;
        this.textures[index].SourceRect = sourceRect;
        this.textures[index].Color = color;
    }

    private void DisableTexture(int index)
    {
        this.textures[index].IsEnabled = false;
    }
}
