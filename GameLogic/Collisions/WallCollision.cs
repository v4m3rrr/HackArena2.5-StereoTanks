namespace GameLogic;

/// <summary>
/// Represents a collision with a wall.
/// </summary>
/// <param name="wall">The wall the collision occurred with.</param>
internal class WallCollision(Wall wall) : ICollision
{
    /// <inheritdoc/>
    public CollisionType Type => CollisionType.Wall;

    /// <summary>
    /// Gets the wall the collision occurred with.
    /// </summary>
    public Wall Wall { get; } = wall;
}
