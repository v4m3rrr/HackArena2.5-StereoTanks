using GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    private static readonly ScalableTexture2D.Static HealingBulletStaticTexture = new("Images/Game/PlayerBarIcons/healing_bullet.svg");
    private static readonly ScalableTexture2D.Static StunBulletStaticTexture = new("Images/Game/PlayerBarIcons/stun_bullet.svg");

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

        var tank = player.Tank;
        List<(IRegenerable, ScalableTexture2D.Static)> regenerables = [];

        if (tank is LightTank light)
        {
            regenerables.Add((light.Turret.DoubleBullet!, DoubleBulletStaticTexture));
            regenerables.Add((light.Radar!, RadarStaticTexture));
        }
        else if (tank is HeavyTank heavy)
        {
            regenerables.Add((heavy.Turret.Laser!, LaserStaticTexture));
            regenerables.Add((heavy.Mine!, MineStaticTexture));
        }

        regenerables.Add((tank.Turret.HealingBullet!, HealingBulletStaticTexture));
        regenerables.Add((tank.Turret.StunBullet!, StunBulletStaticTexture));

        foreach (var (regenerable, staticTexture) in regenerables)
        {
            _ = new Item(player, staticTexture, () => regenerable.RegenerationProgress)
            {
                Parent = this.listBox.ContentContainer,
            };
        }

        this.listBox.Components.Last().Transform.SizeChanged += (s, e) =>
        {
            var size = (s as Transform)!.Size;

            DoubleBulletStaticTexture.Transform.Size = size;
            LaserStaticTexture.Transform.Size = size;
            RadarStaticTexture.Transform.Size = size;
            MineStaticTexture.Transform.Size = size;
#if STEREO
            HealingBulletStaticTexture.Transform.Size = size;
            StunBulletStaticTexture.Transform.Size = size;
#endif
        };
    }

    /// <inheritdoc/>
    public static void LoadStaticContent()
    {
        DoubleBulletStaticTexture.Load();
        LaserStaticTexture.Load();
        RadarStaticTexture.Load();
        MineStaticTexture.Load();
#if STEREO
        HealingBulletStaticTexture.Load();
        StunBulletStaticTexture.Load();
#endif
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
        if (this.Player.IsTankDead)
        {
            return;
        }

        base.Draw(gameTime);
    }

    private class Item : Component
    {
        private readonly Effect effect;
        private readonly VertexPositionTexture[] vertices;

        private readonly Player player;
        private readonly Color playerColor;
        private readonly Func<float?> getProgress;
        private float textureOpacity = 1f;

        public Item(Player player, ScalableTexture2D.Static staticTexture, Func<float?> getProgress)
        {
#if WINDOWS
            this.effect = ContentController.Content.Load<Effect>("Shaders/AngleMask_DX");
#else
            this.effect = ContentController.Content.Load<Effect>("Shaders/AngleMask_GL");
#endif
            this.vertices = new VertexPositionTexture[4];

            this.player = player;
            this.playerColor = new Color(player.Color);
            this.getProgress = getProgress;

            this.Background = this.CreateBackground();

            this.Texture = new ScalableTexture2D(staticTexture)
            {
                Parent = this.Background,
                AutoDraw = false,
                Color = Color.White,
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

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            base.Draw(gameTime);

            var spriteBatch = SpriteBatchController.SpriteBatch;
            var graphicsDevice = ScreenController.GraphicsDevice;
            var progress = this.getProgress();

            if (progress < 1f)
            {
                spriteBatch.End();

                var dest = this.Background.Transform.DestRectangle;
                var vp = graphicsDevice.Viewport;
                float l = ((dest.Left + 0.5f) / vp.Width * 2f) - 1f;
                float r = ((dest.Right - 0.5f) / vp.Width * 2f) - 1f;
                float t = -(((dest.Top + 0.5f) / vp.Height * 2f) - 1f);
                float b = -(((dest.Bottom - 0.5f) / vp.Height * 2f) - 1f);

                this.vertices[0] = new VertexPositionTexture(new Vector3(l, t, 0), new Vector2(0, 0));
                this.vertices[1] = new VertexPositionTexture(new Vector3(r, t, 0), new Vector2(1, 0));
                this.vertices[2] = new VertexPositionTexture(new Vector3(l, b, 0), new Vector2(0, 1));
                this.vertices[3] = new VertexPositionTexture(new Vector3(r, b, 0), new Vector2(1, 1));

                var oldRasterizerState = graphicsDevice.RasterizerState;
                var oldBlendState = graphicsDevice.BlendState;
                var oldDepthStencilState = graphicsDevice.DepthStencilState;
                var oldRenderTarget = graphicsDevice.GetRenderTargets();

                graphicsDevice.RasterizerState = RasterizerState.CullNone;
                graphicsDevice.BlendState = BlendState.NonPremultiplied;
                graphicsDevice.DepthStencilState = DepthStencilState.None;
                graphicsDevice.SetRenderTarget(null);

                this.effect.Parameters["SpriteTexture"].SetValue(this.Background.Texture);
                this.effect.Parameters["GlobalOpacity"].SetValue(1.0f);

                List<(float, Vector4)> segments = progress is null
                    ? [(1f, this.playerColor.ToVector4())]
                    : [(progress.Value, this.playerColor.ToVector4() * 0.75f), (1f - progress.Value, (Color.White * 0.3f).ToVector4())];

                this.effect.Parameters["NumSegments"].SetValue(segments.Count);
                this.effect.Parameters["Pct"].SetValue(segments.Select(s => s.Item1).ToArray());
                this.effect.Parameters["ColorArr"].SetValue(segments.Select(s => s.Item2).ToArray());

                this.effect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, this.vertices, 0, 2);

                dest = this.Texture.Transform.DestRectangle;
                vp = graphicsDevice.Viewport;
                l = (dest.Left / (float)vp.Width * 2f) - 1f;
                r = (dest.Right / (float)vp.Width * 2f) - 1f;
                t = -((dest.Top / (float)vp.Height * 2f) - 1f);
                b = -((dest.Bottom / (float)vp.Height * 2f) - 1f);

                this.vertices[0] = new VertexPositionTexture(new Vector3(l, t, 0), new Vector2(0, 0));
                this.vertices[1] = new VertexPositionTexture(new Vector3(r, t, 0), new Vector2(1, 0));
                this.vertices[2] = new VertexPositionTexture(new Vector3(l, b, 0), new Vector2(0, 1));
                this.vertices[3] = new VertexPositionTexture(new Vector3(r, b, 0), new Vector2(1, 1));

                this.effect.Parameters["SpriteTexture"].SetValue(this.Texture.Texture);
                segments = progress is null
                    ? [(1.1f, Color.White.ToVector4())]
                    : [(progress.Value, Color.White.ToVector4() * 0.9f), (1f - progress.Value, (Color.White * 0.5f).ToVector4())];

                this.effect.Parameters["NumSegments"].SetValue(segments.Count);
                this.effect.Parameters["Pct"].SetValue(segments.Select(s => s.Item1).ToArray());
                this.effect.Parameters["ColorArr"].SetValue(segments.Select(s => s.Item2).ToArray());

                this.effect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, this.vertices, 0, 2);

                graphicsDevice.BlendState = oldBlendState;
                graphicsDevice.DepthStencilState = oldDepthStencilState;
                graphicsDevice.RasterizerState = oldRasterizerState;
                graphicsDevice.SetRenderTargets(oldRenderTarget);

                spriteBatch.Begin(
                    blendState: BlendState.NonPremultiplied,
                    transformMatrix: ScreenController.TransformMatrix);
            }
            else
            {
                this.Background.Draw(gameTime);
                this.Texture.Draw(gameTime);
            }
        }

        private RoundedSolidColor CreateBackground()
        {
            var radius = Math.Min(this.Transform.Size.X, this.Transform.Size.Y) / 5;
            return new RoundedSolidColor(this.playerColor, radius)
            {
                Parent = this,
                AutoDraw = false,
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
