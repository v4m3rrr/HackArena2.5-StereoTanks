namespace GameLogic;

/// <summary>
/// Represents a wall.
/// </summary>
public class Wall
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Wall"/> class.
    /// </summary>
    internal Wall()
    {
    }

    /// <summary>
    /// Gets the X position of the wall.
    /// </summary>
    public int X { get; internal init; }

    /// <summary>
    /// Gets the Y position of the wall.
    /// </summary>
    public int Y { get; internal init; }
}
