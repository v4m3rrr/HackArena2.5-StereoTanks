namespace GameLogic;

/// <summary>
/// Represents a collision with a tank.
/// </summary>
/// <param name="tank">The tank the collision occurred with.</param>
internal class TankCollision(Tank tank) : ICollision
{
    /// <inheritdoc/>
    public CollisionType Type => CollisionType.Tank;

    /// <summary>
    /// Gets the tank the collision occurred with.
    /// </summary>
    public Tank Tank { get; } = tank;
}
