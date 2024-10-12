using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a mine with an explosion animation.
/// </summary>
internal class Mine : Sprite, IDetectableByRadar
{
    private static readonly List<Texture2D> ExplosionTextures = [];
    private static readonly ScalableTexture2D.Static InnerStaticTexture;
    private static readonly ScalableTexture2D.Static OuterStaticTexture;

    private readonly ScalableTexture2D innerTexture;
    private readonly ScalableTexture2D outerTexture;
    private readonly GridComponent grid;

    private Rectangle destRectangle;
    private float explosionProgress;

    static Mine()
    {
        InnerStaticTexture = new ScalableTexture2D.Static("Images/Game/mine_inner.svg");
        InnerStaticTexture.Load();

        OuterStaticTexture = new ScalableTexture2D.Static("Images/Game/mine_outer.svg");
        OuterStaticTexture.Load();

        var content = ContentController.Content;
        var directory = $"Animations/MineExplosion/";
        var files = Directory.GetFiles($"{content.RootDirectory}/{directory}").ToList();
        files.Sort();

        foreach (var file in files)
        {
            var filename = Path.GetFileNameWithoutExtension(file);
            var dupa = directory + filename;
            var texture = content.Load<Texture2D>(dupa);
            ExplosionTextures.Add(texture);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mine"/> class.
    /// </summary>
    /// <param name="logic">The logic of the mine.</param>
    /// <param name="grid">The grid component that the mine will be drawn on.</param>
    public Mine(GameLogic.Mine logic, GridComponent grid)
    {
        this.Logic = logic;

        this.innerTexture = new ScalableTexture2D(InnerStaticTexture)
        {
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize, grid.TileSize),
            },
        };

        this.outerTexture = new ScalableTexture2D(OuterStaticTexture)
        {
            Color = this.Color,
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize, grid.TileSize),
            },
        };

        this.grid = grid;
        this.grid.DrawDataChanged += (s, e) => this.UpdateDestination();
        this.UpdateDestination();
    }

    /// <inheritdoc/>
    float IDetectableByRadar.Opacity
    {
        get => this.outerTexture.Opacity;
        set => this.outerTexture.Opacity = this.innerTexture.Opacity = value;
    }

    /// <summary>
    /// Gets the logic of the mine.
    /// </summary>
    public GameLogic.Mine Logic { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the mine is fully exploded.
    /// </summary>
    /// <remarks>
    /// The mine is fully exploded if it has exploded,
    /// the explosion and the animation have finished.
    /// </remarks>
    public bool IsFullyExploded => this.Logic.IsFullyExploded && this.explosionProgress >= 1;

    private Color Color => this.Logic.Layer is not null
        ? new Color(this.Logic.Layer.Color)
        : MonoTanks.ThemeColor;

    /// <summary>
    /// Updates the logic of the mine.
    /// </summary>
    /// <param name="logic">The new logic of the mine.</param>
    public void UpdateLogic(GameLogic.Mine logic)
    {
        this.Logic = logic;
        this.outerTexture.Color = this.Color;
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.Logic.IsExploded)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            this.explosionProgress += elapsedTime / (GameLogic.Mine.ExplosionTicks * Scenes.Game.ServerBroadcastInterval);
            this.destRectangle = new Rectangle(
                this.outerTexture.Transform.Location - (this.outerTexture.Transform.Size / new Point(2)),
                this.outerTexture.Transform.Size * new Point(2));
        }
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        if (this.Logic.IsExploded)
        {
            var index = (int)(this.explosionProgress * ExplosionTextures.Count);
            if (index < ExplosionTextures.Count)
            {
                var spriteBatch = SpriteBatchController.SpriteBatch;
                var texture = ExplosionTextures[index];
                spriteBatch.Draw(texture, this.destRectangle, Color.White);
            }
        }
        else
        {
            this.outerTexture.Draw(gameTime);
            this.innerTexture.Draw(gameTime);
        }
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

        InnerStaticTexture.Transform.Size = OuterStaticTexture.Transform.Size = new Point(tileSize);
        this.innerTexture.Transform.Location = this.outerTexture.Transform.Location = location;
    }
}
