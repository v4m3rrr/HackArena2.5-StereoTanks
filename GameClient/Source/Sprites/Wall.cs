using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Sprites;

/// <summary>
/// Represents a wall sprite.
/// </summary>
internal abstract class Wall : ISprite
{
    private static readonly ScalableTexture2D.Static StaticTexture = new("Images/Game/wall.svg");

    private readonly ScalableTexture2D texture;
    private readonly GridComponent grid;

    private Wall(GridComponent grid)
    {
        this.texture = new ScalableTexture2D(StaticTexture)
        {
            Transform =
            {
                Type = TransformType.Absolute,
                Size = new Point(grid.TileSize, grid.TileSize),
            },
        };

        this.grid = grid;
        this.grid.DrawDataChanged += (s, e) => this.UpdateDestination();
    }

    /// <summary>
    /// Gets the wall position.
    /// </summary>
    protected abstract Point Position { get; }

    /// <inheritdoc/>
    public static void LoadContent()
    {
        StaticTexture.Load();
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

    private void UpdateDestination()
    {
        int tileSize = this.grid.TileSize;
        int drawOffset = this.grid.DrawOffset;
        int gridLeft = this.grid.Transform.DestRectangle.Left;
        int gridTop = this.grid.Transform.DestRectangle.Top;

        var location = new Point(
             gridLeft + (this.Position.X * tileSize) + drawOffset,
             gridTop + (this.Position.Y * tileSize) + drawOffset);

        StaticTexture.Transform.Size = new Point(tileSize);

        this.texture.Transform.Location = location;
        this.texture.Transform.Size = new Point(tileSize);
    }

    /// <summary>
    /// Represents a border wall.
    /// </summary>
    /// <remarks>
    /// A border wall is a wall that is placed on the border of the grid.
    /// </remarks>
    internal class Border : Wall
    {
        private int x;
        private int y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Border"/> class.
        /// </summary>
        /// <param name="x">The x position of the wall.
        /// </param>
        /// <param name="y">The y position of the wall.</param>
        /// <param name="grid">The grid component.</param>
        public Border(int x, int y, GridComponent grid)
            : base(grid)
        {
            this.x = x;
            this.y = y;
            this.texture.Opacity = 0.35f;

            this.UpdateDestination();
        }

        /// <inheritdoc/>
        protected override Point Position => new(this.x, this.y);
    }

    /// <summary>
    /// Represents a solid wall.
    /// </summary>
    /// <remarks>
    /// A solid wall is a wall that is placed on the grid.
    /// </remarks>
    internal class Solid : Wall
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Solid"/> class.
        /// </summary>
        /// <param name="logic">The wall logic.</param>
        /// <param name="grid">The grid component.</param>
        public Solid(GameLogic.Wall logic, GridComponent grid)
            : base(grid)
        {
            this.Logic = logic;
            this.texture.Opacity = 1f;

            this.UpdateDestination();
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
        }
    }
}
