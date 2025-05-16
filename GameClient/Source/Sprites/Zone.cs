using GameLogic;
using GameLogic.ZoneStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a zone sprite.
/// </summary>
internal class Zone : ISprite
{
    private const float TextureOpacity = 0.7f;
    private const float IndexOpacity = 0.65f;

#if STEREO
    private static readonly ScalableTexture2D.Static StaticTexture = new($"Images/Game/zone.svg");
#else
    private static readonly ScalableTexture2D.Static StaticCornerTexture = new($"Images/Game/zone_corner.svg");
    private static readonly ScalableTexture2D.Static StaticEdgeTexture = new($"Images/Game/zone_edge.svg");
#endif
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 35)
    {
        AutoResize = true,
    };

#if STEREO
    private readonly Effect effect;
    private readonly ScalableTexture2D texture;
    private readonly VertexPositionTexture[] vertices;
#else
    private readonly ScalableTexture2D[] textures = new ScalableTexture2D[8];
#endif

    private readonly GridComponent grid;
    private readonly Text index;

    /// <summary>
    /// Initializes a new instance of the <see cref="Zone"/> class.
    /// </summary>
    /// <param name="logic">The zone logic.</param>
    /// <param name="grid">The grid component.</param>
    public Zone(GameLogic.Zone logic, GridComponent grid)
    {
#if STEREO
        this.texture = new ScalableTexture2D(StaticTexture)
        {
            Opacity = TextureOpacity,
            Transform =
            {
                Type = TransformType.Absolute,
            },
        };

#if WINDOWS
        this.effect = ContentController.Content.Load<Effect>("Shaders/AngleMask_DX");
#else
        this.effect = ContentController.Content.Load<Effect>("Shaders/AngleMask_GL");
#endif
        this.vertices = new VertexPositionTexture[4];
#else
        for (int i = 0; i < 4; i++)
        {
            this.textures[i * 2] = new ScalableTexture2D(StaticEdgeTexture)
            {
                Rotation = MathF.PI / 2 * i,
                RelativeOrigin = new Vector2(0.5f),
                CenterOrigin = true,
                Opacity = TextureOpacity,
                Transform =
                {
                    Type = TransformType.Absolute,
                },
            };

            this.textures[(i * 2) + 1] = new ScalableTexture2D(StaticCornerTexture)
            {
                Rotation = MathF.PI / 2 * i,
                RelativeOrigin = new Vector2(0.5f),
                CenterOrigin = true,
                Opacity = TextureOpacity,
                Transform =
                {
                    Type = TransformType.Absolute,
                },
            };
        }
#endif

        this.index = new Text(Font, Color.White * IndexOpacity)
        {
            Value = logic.Index.ToString(),
            Transform = { Type = TransformType.Absolute },
            TextAlignment = Alignment.Center,
            TextShrink = TextShrinkMode.HeightAndWidth,
        };

        this.Logic = logic;
        this.grid = grid;
        this.grid.DrawDataChanged += (s, e) => this.UpdateDestination();
        this.UpdateDestination();
    }

    /// <summary>
    /// Gets the tank logic.
    /// </summary>
    public GameLogic.Zone Logic { get; private set; }

    /// <inheritdoc/>
    public static void LoadContent()
    {
#if STEREO
        StaticTexture.Load();
#else
        StaticCornerTexture.Load();
        StaticEdgeTexture.Load();
#endif
    }

    /// <summary>
    /// Updates the zone logic.
    /// </summary>
    /// <param name="logic">The new zone logic.</param>
    public void UpdateLogic(GameLogic.Zone logic)
    {
        this.Logic = logic;
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
    {
#if !STEREO

        if (this.Logic.State is NeutralZoneState)
        {
            this.index.Color = Color.White * IndexOpacity;
            foreach (ScalableTexture2D texture in this.textures)
            {
                texture.Color = Color.White;
            }

            return;
        }

        if (this.Logic.State is BeingCapturedZoneState and ICaptureState beingCaptured)
        {
            var color = new Color(beingCaptured.BeingCapturedBy.Color);

            float progress = beingCaptured.RemainingTicks / (float)ZoneSystem.TicksToCapture;
            float progressIndex = progress * this.textures.Length;
            float lastImageProgress = Math.Abs(((this.textures.Length - progressIndex) % 1) - 1) % 1;

            this.index.Color = Color.White * IndexOpacity;

            for (int i = 0; i <= progressIndex - 1; i++)
            {
                this.textures[i].Color = color;
            }

            this.textures[(int)progressIndex].Color = Color.Lerp(Color.White, color, lastImageProgress);

            for (int i = (int)progressIndex + 1; i < this.textures.Length; i++)
            {
                this.textures[i].Color = Color.White;
            }

            return;
        }

        if (this.Logic.State is CapturedZoneState captured)
        {
            var color = new Color(captured.Player.Color);
            this.index.Color = color * IndexOpacity;
            foreach (ScalableTexture2D texture in this.textures)
            {
                texture.Color = color;
            }
        }

        if (this.Logic.State is BeingContestedZoneState beingContested)
        {
            var color = beingContested.CapturedBy is not null ? new Color(beingContested.CapturedBy.Color) : Color.White;
            this.index.Color = color * IndexOpacity;
            for (int i = 0; i < 4; i++)
            {
                this.textures[i * 2].Color = color;
                this.textures[(i * 2) + 1].Color = Color.White;
            }
        }

        if (this.Logic.State is BeingRetakenZoneState beingRetaken)
        {
            var capturedColor = new Color(beingRetaken.CapturedBy.Color);
            var retakenColor = new Color(beingRetaken.RetakenBy.Color);
            this.index.Color = capturedColor * IndexOpacity;

            float progress = beingRetaken.RemainingTicks / (float)ZoneSystem.TicksToCapture;
            float progressIndex = progress * this.textures.Length;
            float lastImageProgress = Math.Abs(((this.textures.Length - progressIndex) % 1) - 1) % 1;

            for (int i = 0; i <= progressIndex - 1; i++)
            {
                this.textures[i].Color = retakenColor;
            }

            this.textures[(int)progressIndex].Color = Color.Lerp(Color.White, new Color(beingRetaken.RetakenBy.Color), lastImageProgress);

            for (int i = (int)progressIndex + 1; i < this.textures.Length; i++)
            {
                this.textures[i].Color = capturedColor;
            }
        }

#endif

        this.index.Update(gameTime);
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
#if STEREO

        var spriteBatch = SpriteBatchController.SpriteBatch;
        var graphicsDevice = ScreenController.GraphicsDevice;
        var dest = this.texture.Transform.DestRectangle;

        spriteBatch.End();

        var vp = graphicsDevice.Viewport;
        float l = (dest.Left / (float)vp.Width * 2f) - 1f;
        float r = (dest.Right / (float)vp.Width * 2f) - 1f;
        float t = -((dest.Top / (float)vp.Height * 2f) - 1f);
        float b = -((dest.Bottom / (float)vp.Height * 2f) - 1f);

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

        const int MaxSegments = 8;

        this.effect.Parameters["SpriteTexture"].SetValue(this.texture.Texture);
        this.effect.Parameters["GlobalOpacity"].SetValue(TextureOpacity);

        var segments = this.Logic.Shares.NormalizedByTeam
            .Select(kvp =>
            {
                Team team = kvp.Key;
                float share = kvp.Value;
                Vector4 color = new Color(team.Color).ToVector4();
                return (Share: share, Color: color);
            })
            .ToList();

        if (segments.Count > MaxSegments - 1)
        {
            segments = [.. segments.Take(MaxSegments - 1)];
        }

        int middleIndex = segments.Count / 2;
        segments.Insert(
            middleIndex,
            (Share: this.Logic.Shares.NormalizedNeutral, Color: Color.White.ToVector4()));

        this.effect.Parameters["NumSegments"].SetValue(segments.Count);
        this.effect.Parameters["Pct"].SetValue(segments.Select(s => s.Share).ToArray());
        this.effect.Parameters["ColorArr"].SetValue(segments.Select(s => s.Color).ToArray());

        this.effect.CurrentTechnique.Passes[0].Apply();
        graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, this.vertices, 0, 2);

        graphicsDevice.BlendState = oldBlendState;
        graphicsDevice.DepthStencilState = oldDepthStencilState;
        graphicsDevice.RasterizerState = oldRasterizerState;
        graphicsDevice.SetRenderTargets(oldRenderTarget);

        spriteBatch.Begin(
            blendState: BlendState.NonPremultiplied,
            transformMatrix: ScreenController.TransformMatrix);

        this.index.Draw(gameTime);

#else
        foreach (ScalableTexture2D texture in this.textures)
        {
            texture.Draw(gameTime);
        }

        this.index.Draw(gameTime);

#endif
    }

    private void UpdateDestination()
    {
        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        var location = new Point(
            gridLeft + (this.Logic.X * tileSize) + drawOffset,
            gridTop + (this.Logic.Y * tileSize) + drawOffset);

        int width = tileSize * this.Logic.Width;
        int height = tileSize * this.Logic.Height;

#if STEREO
        StaticTexture.Transform.Size = new Point(width, height);
        this.texture.Transform.Location = location;
        this.texture.Transform.Size = new Point(width, height);
#else
        StaticCornerTexture.Transform.Size = new Point(width, height);
        StaticEdgeTexture.Transform.Size = new Point(width, height);

        foreach (ScalableTexture2D texture in this.textures)
        {
            texture.Transform.Location = location;
            texture.Transform.Size = new Point(width, height);
        }
#endif

        Font.Size = 35 * 22 / this.grid.Logic.Dim;

        this.index.Transform.Location = location;
        this.index.Transform.Size = new Point(width, height);
        this.index.ForceUpdate(withTransform: true);
    }
}
