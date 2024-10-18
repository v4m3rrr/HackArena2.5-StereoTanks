namespace GameLogic;

/// <summary>
/// Represents a collision with a tank.
/// </summary>
/// <param name="Tank">The tank the collision occurred with.</param>
internal record class TankCollision(Tank Tank) : Collision(CollisionType.Tank);
