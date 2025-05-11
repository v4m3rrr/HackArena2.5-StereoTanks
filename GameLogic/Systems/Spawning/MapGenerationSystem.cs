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
        var wallMask = this.generator.GenerateWalls(zones);

        grid.Zones.Clear();
        grid.Zones.AddRange(zones);

        for (int x = 0; x < grid.Dim; x++)
        {
            for (int y = 0; y < grid.Dim; y++)
            {
                grid.WallGrid[x, y] = wallMask[x, y] ? new Wall() { X = x, Y = y } : null;
            }
        }

        if (onWarning is not null)
        {
            this.generator.GenerationWarning -= onWarning;
        }
    }
}
