namespace GameLogic;

/// <summary>
/// Represents the collision with a bullet.
/// </summary>
/// <param name="bullet">The bullet the collision occurred with.</param>
internal class BulletCollision(Bullet bullet) : ICollision
{
    /// <inheritdoc/>
    public CollisionType Type => CollisionType.Bullet;

    /// <summary>
    /// Gets the bullet the collision occurred with.
    /// </summary>
    public Bullet Bullet { get; } = bullet;
}
