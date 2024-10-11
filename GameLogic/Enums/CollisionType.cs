namespace GameLogic;

/// <summary>
/// Represents the type of collision.
/// </summary>
public enum CollisionType
{
    /// <summary>
    /// No collision.
    /// </summary>
    None,

    /// <summary>
    /// Collision with a border.
    /// </summary>
    Border,

    /// <summary>
    /// Collision with a wall.
    /// </summary>
    Wall,

    /// <summary>
    /// Collision with a tank.
    /// </summary>
    Tank,

    /// <summary>
    /// Collision with a bullet.
    /// </summary>
    Bullet,

    /// <summary>
    /// Collision with a laser.
    /// </summary>
    Laser,
}
