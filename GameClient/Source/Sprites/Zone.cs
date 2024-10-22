using System;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a zone sprite.
/// </summary>
internal class Zone : ISprite
{
    private const float TextureOpacity = 0.7f;
    private const float IndexOpacity = 0.65f;

    private static readonly ScalableTexture2D.Static StaticCornerTexture = new($"Images/Game/zone_corner.svg");
    private static readonly ScalableTexture2D.Static StaticEdgeTexture = new($"Images/Game/zone_edge.svg");
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 25);

    private readonly ScalableTexture2D[] textures = new ScalableTexture2D[8];
    private readonly GridComponent grid;
    private readonly Text index;

    /// <summary>
    /// Initializes a new instance of the <see cref="Zone"/> class.
    /// </summary>
    /// <param name="logic">The zone logic.</param>
    /// <param name="grid">The grid component.</param>
    public Zone(GameLogic.Zone logic, GridComponent grid)
    {
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

        this.index = new Text(Font, Color.White * 0.5f)
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
        StaticCornerTexture.Load();
        StaticEdgeTexture.Load();
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
        if (this.Logic.Status is ZoneStatus.Neutral)
        {
            this.index.Color = Color.White * IndexOpacity;
            foreach (ScalableTexture2D texture in this.textures)
            {
                texture.Color = Color.White;
            }

            return;
        }

        if (this.Logic.Status is ZoneStatus.BeingCaptured beingCaptured)
        {
            var color = new Color(beingCaptured.Player.Color);
            float progress = 1 - ((float)beingCaptured.RemainingTicks / GameLogic.Zone.TicksToCapture);
            float progressIndex = progress * this.textures.Length;
            float lastImageProgress = Math.Abs(((this.textures.Length - progressIndex) % 1) - 1) % 1;

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

        if (this.Logic.Status is ZoneStatus.Captured captured)
        {
            var color = new Color(captured.Player.Color);
            this.index.Color = color * IndexOpacity;
            foreach (ScalableTexture2D texture in this.textures)
            {
                texture.Color = color;
            }
        }

        if (this.Logic.Status is ZoneStatus.BeingContested beingContested)
        {
            var color = beingContested.CapturedBy is not null ? new Color(beingContested.CapturedBy.Color) : Color.White;
            this.index.Color = color * IndexOpacity;
            for (int i = 0; i < 4; i++)
            {
                this.textures[i * 2].Color = color;
                this.textures[(i * 2) + 1].Color = Color.White;
            }
        }

        if (this.Logic.Status is ZoneStatus.BeingRetaken beingRetaken)
        {
            var capturedColor = new Color(beingRetaken.CapturedBy.Color);
            var retakenColor = new Color(beingRetaken.RetakenBy.Color);
            this.index.Color = capturedColor * IndexOpacity;

            float progress = 1 - ((float)beingRetaken.RemainingTicks / GameLogic.Zone.TicksToCapture);
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
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
        foreach (ScalableTexture2D texture in this.textures)
        {
            texture.Draw(gameTime);
        }

        this.index.Draw(gameTime);
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

        StaticCornerTexture.Transform.Size = new Point(width, height);
        StaticEdgeTexture.Transform.Size = new Point(width, height);

        foreach (ScalableTexture2D texture in this.textures)
        {
            texture.Transform.Location = location;
            texture.Transform.Size = new Point(width, height);
        }

        this.index.Transform.Location = location;
        this.index.Transform.Size = new Point(width, height);
    }
}
