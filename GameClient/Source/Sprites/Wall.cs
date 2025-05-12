using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a wall sprite.
/// </summary>
internal abstract class Wall : ISprite
{
    private static readonly ScalableTexture2D.Static StaticTexture = new("Images/Game/wall.svg");
#if STEREO
    private static readonly ScalableTexture2D.Static PenetrableStaticTexture = new("Images/Game/penetrable_wall.svg");
#endif

    private readonly ScalableTexture2D texture;
    private readonly GridComponent grid;

    private Wall(GridComponent grid, ScalableTexture2D.Static staticTexture)
    {
        this.texture = new ScalableTexture2D(staticTexture)
        {
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize, grid.TileSize),
            },
        };

        this.grid = grid;
        this.grid.DrawDataChanged += (s, e) =>
        {
            this.UpdateSize();
            this.UpdateLocation();
        };
    }

    /// <summary>
    /// Gets the wall position.
    /// </summary>
    protected abstract Point Position { get; }

#if STEREO

    /// <summary>
    /// Creates a wall from the given logic.
    /// </summary>
    /// <param name="logic">The wall logic.</param>
    /// <param name="grid">The grid component.</param>
    /// <returns>The wall created from the logic.</returns>
    public static WallWithLogic? FromType(GameLogic.Wall logic, GridComponent grid)
    {
        return logic.Type switch
        {
            WallType.Solid => new Solid(logic, grid),
            WallType.Penetrable => new Penetrable(logic, grid),
            _ => null,
        };
    }

#endif

    /// <inheritdoc/>
    public static void LoadContent()
    {
        StaticTexture.Load();
#if STEREO
        PenetrableStaticTexture.Load();
#endif
    }

    /// <inheritdoc/>
    public void Update(GameTime gameTime)
    {
    }

    /// <inheritdoc/>
    public void Draw(GameTime gameTime)
    {
        this.texture.Draw(gameTime);
    }

    private void UpdateSize()
    {
        int tileSize = this.grid.TileSize;
        StaticTexture.Transform.Size = new Point(tileSize);
#if STEREO
        PenetrableStaticTexture.Transform.Size = new Point(tileSize);
#endif
        this.texture.Transform.Size = new Point(tileSize);
    }

    private void UpdateLocation()
    {
        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        var location = new Point(
             gridLeft + (this.Position.X * tileSize) + drawOffset,
             gridTop + (this.Position.Y * tileSize) + drawOffset);

        this.texture.Transform.Location = location;
    }

    /// <summary>
    /// Represents a border wall.
    /// </summary>
    /// <remarks>
    /// A border wall is a wall that is placed on the border of the grid.
    /// </remarks>
    internal class Border : Wall
    {
        private readonly int x;
        private readonly int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Border"/> class.
        /// </summary>
        /// <param name="x">The x position of the wall.
        /// </param>
        /// <param name="y">The y position of the wall.</param>
        /// <param name="grid">The grid component.</param>
        public Border(int x, int y, GridComponent grid)
            : base(grid, StaticTexture)
        {
            this.x = x;
            this.y = y;
            this.texture.Opacity = 0.35f;

            this.UpdateSize();
            this.UpdateLocation();
        }

        /// <inheritdoc/>
        protected override Point Position => new(this.x, this.y);
    }

    /// <summary>
    /// Represents a wall with logic.
    /// </summary>
    internal abstract class WallWithLogic : Wall
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WallWithLogic"/> class.
        /// </summary>
        /// <param name="logic">The wall logic.</param>
        /// <param name="grid">The grid component.</param>
        /// <param name="staticTexture">The static texture of the wall.</param>
        protected WallWithLogic(
            GameLogic.Wall logic,
            GridComponent grid,
            ScalableTexture2D.Static staticTexture)
            : base(grid, staticTexture)
        {
            this.Logic = logic;
            this.UpdateSize();
            this.UpdateLocation();
        }

        /// <summary>
        /// Gets the wall logic.
        /// </summary>
        public GameLogic.Wall Logic { get; private set; }

        /// <inheritdoc/>
        protected override Point Position => new(this.Logic.X, this.Logic.Y);

        /// <summary>
        /// Updates the wall logic.
        /// </summary>
        /// <param name="logic">The new wall logic.</param>
        public void UpdateLogic(GameLogic.Wall logic)
        {
            this.Logic = logic;
            this.UpdateLocation();
        }
    }

    /// <summary>
    /// Represents a solid wall.
    /// </summary>
    /// <remarks>
    /// A solid wall is a wall that is placed on the grid.
    /// </remarks>
    /// <param name="logic">The wall logic.</param>
    /// <param name="grid">The grid component.</param>
    internal class Solid(GameLogic.Wall logic, GridComponent grid)
        : WallWithLogic(logic, grid, StaticTexture)
    {
    }

#if STEREO

    /// <summary>
    /// Represents a solid wall.
    /// </summary>
    /// <remarks>
    /// A penetrable wall is a wall that is placed on the grid
    /// and bullets can pass through it.
    /// </remarks>
    /// <param name="logic">The wall logic.</param>
    /// <param name="grid">The grid component.</param>
    internal class Penetrable(GameLogic.Wall logic, GridComponent grid)
        : WallWithLogic(logic, grid, PenetrableStaticTexture)
    {
    }

#endif
}
