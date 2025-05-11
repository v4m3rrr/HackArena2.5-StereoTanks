namespace GameLogic.Networking.Map;

/// <summary>
/// Represents a tiles payload for the grid.
/// </summary>
/// <param name="WallGrid">The wall grid of the grid.</param>
/// <param name="Tanks">The tanks of the grid.</param>
/// <param name="Bullets">The bullets of the grid.</param>
/// <param name="Lasers">The lasers on the grid.</param>
/// <param name="Mines">The mines on the grid.</param>
internal record class TilesPayload(
    Wall?[,] WallGrid,
    List<Tank> Tanks,
    List<Bullet> Bullets,
    List<Laser> Lasers,
    List<Mine> Mines)
{
#if !STEREO
    /// <summary>
    /// Gets the items on the grid.
    /// </summary>
    public required List<SecondaryItem> Items { get; init; }
#endif
}
