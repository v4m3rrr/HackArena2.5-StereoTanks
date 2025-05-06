using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

#if STEREO
/// <summary>
/// Represents the secondary items of a player displayed on a player bar.
/// </summary>
internal class Abilities : PlayerBarComponent, ILoadStaticContent
{
    private static readonly ScalableTexture2D.Static LaserStaticTexture = new("Images/Game/PlayerBarIcons/laser.svg");
    private static readonly ScalableTexture2D.Static DoubleBulletStaticTexture = new("Images/Game/PlayerBarIcons/double_bullet.svg");
    private static readonly ScalableTexture2D.Static RadarStaticTexture = new("Images/Game/PlayerBarIcons/radar.svg");
    private static readonly ScalableTexture2D.Static MineStaticTexture = new("Images/Game/PlayerBarIcons/mine.svg");

    private readonly ListBox listBox;

    /// <summary>
    /// Initializes a new instance of the <see cref="Abilities"/> class.
    /// </summary>
    /// <param name="player">The player whose abilities will be displayed.</param>
    public Abilities(Player player)
        : base(player)
    {
        this.listBox = new FlexListBox
        {
            Parent = this,
            Orientation = MonoRivUI.Orientation.Horizontal,
            Transform =
            {
                Alignment = Alignment.Center,
            },
        };

        if (player.Tank is LightTank light)
        {
            // Double bullet
            _ = new Item(player, DoubleBulletStaticTexture, () => light.Turret.DoubleBulletRegenProgress)
            {
                Parent = this.listBox.ContentContainer,
            };

            // Radar
            _ = new Item(player, RadarStaticTexture, () => light.RadarRegenProgress)
            {
                Parent = this.listBox.ContentContainer,
            };
        }
        else if (player.Tank is HeavyTank heavy)
        {
            // Laser
            _ = new Item(player, LaserStaticTexture, () => heavy.Turret.LaserRegenProgress)
            {
                Parent = this.listBox.ContentContainer,
            };

            // Mine
            _ = new Item(player, MineStaticTexture, () => heavy.MineRegenProgress)
            {
                Parent = this.listBox.ContentContainer,
            };
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                nameof(player),
                player.Tank.Type,
                "Tank type not supported.");
        }

        this.listBox.Components.Last().Transform.SizeChanged += (s, e) =>
        {
            var size = (s as Transform)!.Size;
            DoubleBulletStaticTexture.Transform.Size = size;
            LaserStaticTexture.Transform.Size = size;
            RadarStaticTexture.Transform.Size = size;
            MineStaticTexture.Transform.Size = size;
        };
    }

    /// <inheritdoc/>
    public static void LoadStaticContent()
    {
        LaserStaticTexture.Load();
        DoubleBulletStaticTexture.Load();
        RadarStaticTexture.Load();
        MineStaticTexture.Load();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        this.listBox.Spacing = (int)(7 * ScreenController.Scale.X);
        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (this.Player.IsDead)
        {
            return;
        }

        base.Draw(gameTime);
    }

    private class Item : Component
    {
        private readonly Player player;
        private readonly Color playerColor;
        private readonly Func<float?> getProgress;

        public Item(Player player, ScalableTexture2D.Static staticTexture, Func<float?> getProgress)
        {
            this.player = player;
            this.playerColor = new Color(player.Color);
            this.getProgress = getProgress;

            this.Background = this.CreateBackground();

            this.Texture = new ScalableTexture2D(staticTexture)
            {
                Parent = this.Background,
                Transform =
                {
                    Alignment = Alignment.Center,
                    RelativeSize = new Vector2(0.67f),
                },
            };

            this.Transform.SizeChanged += (s, e) =>
            {
                this.Background.Parent = null;
                this.Background = this.CreateBackground();
                this.Background.Load();
                this.Texture.Parent = null;
                this.Texture.Parent = this.Background;
                this.ForceUpdate();
            };
        }

        public RoundedSolidColor Background { get; private set; }

        public ScalableTexture2D Texture { get; }

        public override void Update(GameTime gameTime)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            var progress = this.getProgress();

            if (progress is null)
            {
                if (this.player.IsDead)
                {
                    this.Background.Color = Color.White;
                    this.Background.Opacity = 0.27f;
                    this.Texture.Opacity = 0.44f;
                }
                else
                {
                    this.Background.Color = this.playerColor;
                    this.Texture.Opacity = this.Background.Opacity = 1f;
                }
            }
            else
            {
                this.Background.Color = this.playerColor;
                this.Background.Opacity = MathHelper.Lerp(0.27f, 1f, progress.Value);
                this.Texture.Opacity = MathHelper.Lerp(0.44f, 0.73f, progress.Value);
            }

            base.Update(gameTime);
        }

        private RoundedSolidColor CreateBackground()
        {
            var radius = Math.Min(this.Transform.Size.X, this.Transform.Size.Y) / 5;
            return new RoundedSolidColor(Color.LimeGreen, radius)
            {
                Parent = this,
                Transform =
                {
                    Alignment = Alignment.Center,
                    Ratio = new Ratio(1, 1),
                },
            };
        }
    }
}

#endif
