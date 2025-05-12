namespace GameLogic;

/// <summary>
/// Represents a wall.
/// </summary>
public class Wall(int x, int y)
{
    /// <summary>
    /// Gets the X position of the wall.
    /// </summary>
    public int X { get; } = x;

    /// <summary>
    /// Gets the Y position of the wall.
    /// </summary>
    public int Y { get; } = y;

#if STEREO

    /// <summary>
    /// Gets the type of the wall.
    /// </summary>
    public WallType Type { get; init; }

#endif
}
