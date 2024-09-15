namespace GameLogic;

/// <summary>
/// Represents a collision with a border.
/// </summary>
/// <param name="x">The X position of the border.</param>
/// <param name="y">The Y position of the border.</param>
public class BorderCollision(int x, int y) : ICollision
{
    /// <inheritdoc/>
    public CollisionType Type => CollisionType.Border;

    /// <summary>
    /// Gets the X position of the border.
    /// </summary>
    public int X { get; } = x;

    /// <summary>
    /// Gets the Y position of the border.
    /// </summary>
    public int Y { get; } = y;
}
