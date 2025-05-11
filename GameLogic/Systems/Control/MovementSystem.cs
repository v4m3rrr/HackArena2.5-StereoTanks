namespace GameLogic;

/// <summary>
/// Handles tank movement and collision logic.
/// </summary>
/// <param name="grid">The grid containing the tanks, walls, and other objects.</param>
/// <param name="stunSystem">Stun system to check for stun effects.</param>
internal sealed class MovementSystem(Grid grid, StunSystem stunSystem)
{
    /// <summary>
    /// Attempts to move the tank in the given direction.
    /// </summary>
    /// <param name="tank">The tank to move.</param>
    /// <param name="movement">The movement direction (forward or backward).</param>
    public void TryMoveTank(Tank tank, MovementDirection movement)
    {
        if (stunSystem.IsBlocked(tank, StunBlockEffect.Movement))
        {
            return;
        }

        var (dx, dy) = DirectionUtils.Normal(tank.Direction);
        int step = -((int)movement * 2) + 1;
        int newX = tank.X + (dx * step);
        int newY = tank.Y + (dy * step);

        if (!grid.IsCellWithinBounds(newX, newY))
        {
            return;
        }

        bool blocked = grid.GetCellObjects(newX, newY)
            .Any(obj => obj is Wall or Tank);

        if (!blocked)
        {
            tank.SetPosition(newX, newY);
        }
    }
}
