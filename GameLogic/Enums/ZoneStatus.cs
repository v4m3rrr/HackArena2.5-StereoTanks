namespace GameLogic;

/// <summary>
/// Represents the status of a zone.
/// </summary>
public record class ZoneStatus
{
    /// <summary>
    /// Represents a neutral zone.
    /// </summary>
    public record class Neutral : ZoneStatus;

    /// <summary>
    /// Represents a zone that is being captured.
    /// </summary>
    /// <param name="Player">The player that is capturing the zone.</param>
    /// <param name="RemainingTicks">The number of ticks remaining until the zone is captured.</param>
    public record class BeingCaptured(Player Player, int RemainingTicks) : ZoneStatus
    {
        /// <summary>
        /// Gets the number of ticks remaining until the zone is captured.
        /// </summary>
        public int RemainingTicks { get; internal set; } = RemainingTicks;
    }

    /// <summary>
    /// Represents a zone that is captured.
    /// </summary>
    /// <param name="Player">The player that captured the zone.</param>
    public record class Captured(Player Player) : ZoneStatus;

    /// <summary>
    /// Represents a zone that is being contested.
    /// </summary>
    /// <param name="CapturedBy">The player that captured the zone, if the zone was captured.</param>
    public record class BeingContested(Player? CapturedBy) : ZoneStatus;

    /// <summary>
    /// Represents a zone that is being retaken.
    /// </summary>
    /// <param name="CapturedBy">
    /// The player that captured the zone.
    /// </param>
    /// <param name="RetakenBy">The player that is retaking the zone</param>
    /// <param name="RemainingTicks">The number of ticks remaining until the zone is retaken.</param>
    public record class BeingRetaken(Player CapturedBy, Player RetakenBy, int RemainingTicks) : ZoneStatus
    {
        /// <summary>
        /// Gets the number of ticks remaining until the zone is retaken.
        /// </summary>
        public int RemainingTicks { get; internal set; } = RemainingTicks;
    }
}

