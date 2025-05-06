namespace GameLogic.Networking.GoToElements;

#if STEREO

/// <summary>
/// Represents the penalties associated with different actions in the pathfinding algorithm.
/// </summary>
public class Penalties
{
    /// <summary>
    /// Gets the penalty associated with blindly moving.
    /// </summary>
    /// <remarks>
    /// This value is used only for the first move.
    /// </remarks>
    public float Blindly { get; init; } = 5f;

    /// <summary>
    /// Gets the penalty associated with being hit by a bullet.
    /// </summary>
    public float Bullet { get; init; } = 20f;

    /// <summary>
    /// Gets the penalty associated with stepping on a mine.
    /// </summary>
    public float Mine { get; init; } = 20f;

    /// <summary>
    /// Gets the penalty associated with being hit by a laser.
    /// </summary>
    public float Laser { get; init; } = 20f;

    /// <summary>
    /// Gets the additional penalties assigned to specific tiles on the map.
    /// </summary>
    public List<CustomPenalty> PerTile { get; init; } = [];
}

#endif
