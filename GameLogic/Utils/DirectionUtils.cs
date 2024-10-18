namespace GameLogic;

/// <summary>
/// Represents a direction enum utility.
/// </summary>
public static class DirectionUtils
{
    /// <summary>
    /// Converts a direction to a rotation in radians.
    /// </summary>
    /// <param name="direction">The direction to convert.</param>
    /// <returns>
    /// The rotation in radians.
    /// </returns>
    public static float ToRadians(Direction direction)
    {
        return direction switch
        {
            Direction.Up => 0,
            Direction.Right => MathF.PI / 2f,
            Direction.Down => MathF.PI,
            Direction.Left => MathF.PI * 3f / 2f,
            _ => 0,
        };
    }

    /// <summary>
    /// Converts a direction to a rotation in degrees.
    /// </summary>
    /// <param name="direction">The direction to convert.</param>
    /// <returns>The rotation in degrees.</returns>
    public static int ToDegrees(Direction direction)
    {
        return direction switch
        {
            Direction.Up => 0,
            Direction.Right => 90,
            Direction.Down => 180,
            Direction.Left => 270,
            _ => 0,
        };
    }

    /// <summary>
    /// Converts a direction to a normal vector.
    /// </summary>
    /// <param name="direction">The direction to convert.</param>
    /// <returns>The normal vector.</returns>
    public static (int X, int Y) Normal(Direction direction)
    {
        return direction switch
        {
            Direction.Up => (0, -1),
            Direction.Right => (1, 0),
            Direction.Down => (0, 1),
            Direction.Left => (-1, 0),
            _ => (0, 0),
        };
    }

    /// <summary>
    /// Returns a value indicating whether the two directions are perpendicular.
    /// </summary>
    /// <param name="a">The first direction.</param>
    /// <param name="b">The second direction.</param>
    /// <returns>
    /// <see langword="true"/> if the two directions are perpendicular;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ArePerpendicular(Direction a, Direction b)
    {
        return (int)a % 2 != (int)b % 2;
    }

    /// <summary>
    /// Converts a direction to an orientation.
    /// </summary>
    /// <param name="direction">The direction to convert.</param>
    /// <returns>The orientation.</returns>
    public static Orientation ToOrientation(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Orientation.Vertical,
            Direction.Down => Orientation.Vertical,
            Direction.Left => Orientation.Horizontal,
            Direction.Right => Orientation.Horizontal,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
    }
}
