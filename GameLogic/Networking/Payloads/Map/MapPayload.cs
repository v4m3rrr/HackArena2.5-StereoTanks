namespace GameLogic.Networking.Map;

/// <summary>
/// Represents a map payload for the grid.
/// </summary>
/// <param name="Tiles">The tiles payload for the grid.</param>
/// <param name="Zones">The zones of the grid.</param>
internal record class MapPayload(TilesPayload Tiles, List<Zone> Zones)
{
#if !STEREO

    /* Backwards compatibility */

    /// <summary>
    /// Gets the visibility payload for the grid.
    /// </summary>
    public required VisibilityPayload? Visibility { get; init; }

#endif
}
