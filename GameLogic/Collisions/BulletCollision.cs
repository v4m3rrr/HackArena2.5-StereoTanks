namespace GameLogic;

/// <summary>
/// Represents the collision with a bullet.
/// </summary>
/// <param name="Bullet">The bullet the collision occurred with.</param>
internal record class BulletCollision(Bullet Bullet) : Collision(CollisionType.Bullet);
