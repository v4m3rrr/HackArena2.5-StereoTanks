namespace GameLogic;

/// <summary>
/// An interface for collision.
/// </summary>
public interface ICollision
{
    /// <summary>
    /// Gets the collision type.
    /// </summary>
    CollisionType Type { get; }
}
