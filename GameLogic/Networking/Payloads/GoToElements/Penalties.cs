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
#if CLIENT
    public float? Blindly { get; init; } = 5f;
#elif SERVER
    public float? Blindly { get; init; }
#endif

    /// <summary>
    /// Gets the penalty associated with moving to a taken tile by a tank.
    /// </summary>
#if CLIENT
    public float? Tank { get; init; } = 10f;
#elif SERVER
    public float? Tank { get; init; }
#endif

    /// <summary>
    /// Gets the penalty associated with being hit by a bullet.
    /// </summary>
#if CLIENT
    public float? Bullet { get; init; } = 20f;
#elif SERVER
    public float? Bullet { get; init; }
#endif

    /// <summary>
    /// Gets the penalty associated with stepping on a mine.
    /// </summary>
#if CLIENT
    public float? Mine { get; init; } = 20f;
#elif SERVER
    public float? Mine { get; init; }
#endif

    /// <summary>
    /// Gets the penalty associated with being hit by a laser.
    /// </summary>
#if CLIENT
    public float? Laser { get; init; } = 20f;
#elif SERVER
    public float? Laser { get; init; }
#endif

#if HACKATHON

    /// <summary>
    /// Gets the additional penalties assigned to specific tiles on the map.
    /// </summary>
    public List<CustomPenalty>? PerTile { get; init; }

#endif
}

#endif
