using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

/// <summary>
/// Represents the bullet count of a player displayed on a player bar.
/// </summary>
internal class BulletCount : PlayerBarComponent, ILoadStaticContent
{
    private static readonly ScalableTexture2D.Static StaticTexture = new("Images/Game/bullet.svg");

    private readonly BulletAbility ability;
    private readonly ScalableTexture2D[] textures;
    private readonly ScalableTexture2D[] backgroundTextures;
    private readonly ListBox listBox;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulletCount"/> class.
    /// </summary>
    /// <param name="player">The player whose bullet count will be displayed.</param>
    public BulletCount(Player player)
        : base(player)
    {
        var turret = player.Tank.Turret;

        this.ability = turret.Bullet
            ?? throw new InvalidOperationException("The turret does not have a bullet ability.");

        this.textures = new ScalableTexture2D[BulletAbility.MaxBullets];
        this.backgroundTextures = new ScalableTexture2D[BulletAbility.MaxBullets];

        this.listBox = new ListBox
        {
            Parent = this,
            Orientation = MonoRivUI.Orientation.Horizontal,
            Spacing = 8,
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
    public static void LoadStaticContent()
    {
        StaticTexture.Load();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled || this.Player.IsTankDead)
        {
            return;
        }

        var count = this.ability.Count;
        var regenerationProgress = this.ability.RegenerationProgress;

        for (int i = 0; i < this.textures.Length; i++)
        {
            if (i < count)
            {
                this.EnableTexture(i, null, Color.White);
            }
            else if (regenerationProgress is { } progress && i == count)
            {
                var sourceRect = CalculateBulletRegenSourceRect(progress);
                this.EnableTexture(i, sourceRect, Color.White * 0.75f);
            }
            else
            {
                this.DisableTexture(i);
            }
        }

        this.listBox.Spacing = (int)(10 * ScreenController.Scale.X);

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (!this.IsEnabled || this.Player.IsTankDead)
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
