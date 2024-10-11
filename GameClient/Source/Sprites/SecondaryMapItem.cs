using Microsoft.Xna.Framework;

namespace GameClient.Sprites;

/// <summary>
/// Represents a secondary map item sprite.
/// </summary>
internal abstract class SecondaryMapItem : Sprite
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecondaryMapItem"/> class.
    /// </summary>
    /// <param name="logic">The secondary item logic.</param>
    /// <param name="grid">The grid component.</param>
    protected SecondaryMapItem(GameLogic.SecondaryMapItem logic, GridComponent grid)
    {
        this.Logic = logic;
        this.Grid = grid;
        this.Grid.DrawDataChanged += (s, e) => this.UpdateDestination();
    }

    /// <summary>
    /// Gets the secondary item logic.
    /// </summary>
    protected GameLogic.SecondaryMapItem Logic { get; }

    /// <summary>
    /// Gets the grid component.
    /// </summary>
    protected GridComponent Grid { get; }

    /// <summary>
    /// Gets the size of the map item.
    /// </summary>
    protected Point Size { get; private set; }

    /// <summary>
    /// Gets the location of the map item.
    /// </summary>
    protected Point Location { get; private set; }

    /// <summary>
    /// Updates the destination of the map item.
    /// </summary>
    /// <remarks>
    /// This method saves the result of the
    /// calculation in the <see cref="Location"/>
    /// and <see cref="Size"/> properties.
    /// </remarks>
    protected virtual void UpdateDestination()
    {
        int tileSize = this.Grid.TileSize;
        int drawOffset = this.Grid.DrawOffset;
        int gridLeft = this.Grid.Transform.DestRectangle.Left;
        int gridTop = this.Grid.Transform.DestRectangle.Top;

        this.Location = new Point(
             gridLeft + (this.Logic.X * tileSize) + drawOffset,
             gridTop + (this.Logic.Y * tileSize) + drawOffset);
        this.Size = new Point(tileSize, tileSize);
    }
}
