namespace GameLogic;

/// <summary>
/// Handles procedural map generation (walls, zones, fog of war).
/// </summary>
/// <param name="grid">The grid to generate the map for.</param>
internal sealed class MapGenerationSystem(Grid grid)
{
    private readonly MapGenerator generator = new(grid.Dim, grid.Seed);

    /// <summary>
    /// Generates walls and zones, and updates the fog of war.
    /// </summary>
    /// <param name="onWarning">Optional callback for generation warnings.</param>
    public void GenerateMap(EventHandler<string>? onWarning = null)
    {
        if (onWarning is not null)
        {
            this.generator.GenerationWarning += onWarning;
        }

        var zones = this.generator.GenerateZones();
        var wallGrid = this.generator.GenerateWalls(zones);
        Array.Copy(wallGrid, grid.WallGrid, wallGrid.Length);

        grid.Zones.Clear();
        grid.Zones.AddRange(zones);

        if (onWarning is not null)
        {
            this.generator.GenerationWarning -= onWarning;
        }
    }
}
