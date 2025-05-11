namespace GameLogic;

/// <summary>
/// Represents an entity that can regenerate over time using a tick-based system.
/// </summary>
public interface IRegenerable
{
    /// <summary>
    /// Gets the number of ticks remaining until regeneration is complete.
    /// </summary>
    int? RemainingRegenerationTicks { get; }

    /// <summary>
    /// Gets the total number of ticks required to fully regenerate.
    /// </summary>
    int TotalRegenerationTicks { get; }

    /// <summary>
    /// Gets the normalized regeneration progress from 0.0 to 1.0,
    /// or <see langword="null"/> if already fully regenerated.
    /// </summary>
    float? RegenerationProgress { get; }

    /// <summary>
    /// Decreases the remaining ticks by one,
    /// advancing regeneration progress.
    /// </summary>
    void RegenerateTick();

    /// <summary>
    /// Instantly completes regeneration, setting
    /// the remaining ticks to <see langword="null"/>.
    /// </summary>
    void RegenerateFull();
}
