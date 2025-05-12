namespace GameLogic;

/// <summary>
/// Represents a collision with a wall.
/// </summary>
/// <param name="Wall">The wall the collision occurred with.</param>
internal record class WallCollision(Wall Wall) : Collision(CollisionType.Wall);
