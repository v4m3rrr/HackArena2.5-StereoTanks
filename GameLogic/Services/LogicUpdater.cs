namespace GameLogic;

/// <summary>
/// Represents a logic updater.
/// </summary>
/// <param name="grid">The grid that will be updated.</param>
public class LogicUpdater(Grid grid)
{
    /// <summary>
    /// Gets or sets the bullet delta time.
    /// </summary>
    public float BulletDeltaTime { get; set; } = 1f;

    /// <summary>
    /// Updates the grid.
    /// </summary>
    public void UpdateGrid()
    {
        grid.UpdateBullets(this.BulletDeltaTime);
        grid.RegeneratePlayersBullets();
        grid.UpdateTanksRegenerationProgress();
        grid.UpdatePlayersVisibilityGrids();
        grid.UpdateZones();
        grid.GenerateNewItemOnMap();
    }
}
