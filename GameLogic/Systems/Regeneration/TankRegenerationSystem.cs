namespace GameLogic;

/// <summary>
/// Handles regeneration progress for dead tanks.
/// </summary>
/// <param name="grid">The grid containing the tanks.</param>
internal sealed class TankRegenerationSystem(Grid grid)
{
    /// <summary>
    /// Updates regeneration progress for each player's tank.
    /// </summary>
    public void Update()
    {
        foreach (var tank in grid.Tanks)
        {
            tank.RegenerateTick();
        }
    }
}
