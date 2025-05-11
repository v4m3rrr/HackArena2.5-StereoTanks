namespace GameLogic;

/// <summary>
/// Handles visibility grid calculations for all players.
/// </summary>
/// <param name="grid">The grid to manage visibility for.</param>
internal sealed class VisibilitySystem(Grid grid)
{
    private readonly FogOfWarManager fogOfWarManager = new(grid.WallGrid!);

    /// <summary>
    /// Updates the visibility grid for each player based
    /// on their tank's position and fog-of-war rules.
    /// </summary>
    public void Update()
    {
        foreach (var tank in grid.Tanks)
        {
            this.Update(tank);
        }
    }

    /// <summary>
    /// Updates the visibility grid for a specific tank.
    /// </summary>
    /// <param name="tank">The tank to update the visibility grid for.</param>
    public void Update(Tank tank)
    {
        const int angle = 144;

        if (tank.IsDead)
        {
            tank.VisibilityGrid = this.fogOfWarManager.EmptyGrid;
        }
        else
        {
            var visibilityGrid = this.fogOfWarManager.CalculateVisibilityGrid(tank, angle);
            tank.VisibilityGrid = visibilityGrid;
        }
    }

    /// <summary>
    /// Determines whether the specified position is visible to at least one tank
    /// based on their individual visibility grids.
    /// </summary>
    /// <param name="x">The x-coordinate of the position to check.</param>
    /// <param name="y">The y-coordinate of the position to check.</param>
    /// <returns>
    /// <see langword="true"/> if the position is visible to any tank;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsVisibleByAnyTank(int x, int y)
    {
        return grid.Tanks.Any(tank
            => FogOfWarManager.IsElementVisible(tank.VisibilityGrid, x, y));
    }
}
