using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents the status of a zone.
/// </summary>
public class ZoneStatus
{
    /// <summary>
    /// Represents a neutral zone.
    /// </summary>
    public class Neutral : ZoneStatus
    {
    }

    /// <summary>
    /// Represents a zone that is being captured.
    /// </summary>
    /// <param name="player">The player that is capturing the zone.</param>
    /// <param name="remainingTicks">The number of ticks remaining until the zone is captured.</param>
    public class BeingCaptured(Player player, int remainingTicks) : ZoneStatus
    {
        /// <summary>
        /// Gets the player that is being capturing the zone.
        /// </summary>
        [JsonIgnore]
        public Player Player { get; internal set; } = player;

        /// <summary>
        /// Gets the number of ticks remaining until the zone is captured.
        /// </summary>
        public int RemainingTicks { get; internal set; } = remainingTicks;

        /// <summary>
        /// Gets the ID of the player that is being capturing the zone.
        /// </summary>
        [JsonProperty]
        internal string PlayerId { get; private set; } = player?.Id!;
    }

    /// <summary>
    /// Represents a zone that is captured.
    /// </summary>
    /// <param name="player">The player that captured the zone.</param>
    public class Captured(Player player) : ZoneStatus
    {
        /// <summary>
        /// Gets the player that is capturing the zone.
        /// </summary>
        [JsonIgnore]
        public Player Player { get; internal set; } = player;

        /// <summary>
        /// Gets the ID of the player that is capturing the zone.
        /// </summary>
        [JsonProperty]
        internal string PlayerId { get; private set; } = player?.Id!;
    }

    /// <summary>
    /// Represents a zone that is being contested.
    /// </summary>
    /// <param name="capturedBy">The player that captured the zone, if the zone was captured.</param>
    public class BeingContested(Player? capturedBy) : ZoneStatus
    {
        /// <summary>
        /// Gets the player that is capturing the zone.
        /// </summary>
        [JsonIgnore]
        public Player? CapturedBy { get; internal set; } = capturedBy;

        /// <summary>
        /// Gets the ID of the player that is capturing the zone.
        /// </summary>
        [JsonProperty]
        internal string? CapturedById { get; private set; } = capturedBy?.Id;
    }

    /// <summary>
    /// Represents a zone that is being retaken.
    /// </summary>
    /// <param name="capturedBy">The player that captured the zone.</param>
    /// <param name="retakenBy">The player that is retaking the zone.</param>
    /// <param name="remainingTicks">The number of ticks remaining until the zone is retaken.</param>
    public class BeingRetaken(Player capturedBy, Player retakenBy, int remainingTicks) : ZoneStatus
    {
        /// <summary>
        /// Gets the player that is capturing the zone.
        /// </summary>
        [JsonIgnore]
        public Player CapturedBy { get; internal set; } = capturedBy;

        /// <summary>
        /// Gets the player that is being retaking the zone.
        /// </summary>
        [JsonIgnore]
        public Player RetakenBy { get; internal set; } = retakenBy;

        /// <summary>
        /// Gets the number of ticks remaining until the zone is retaken.
        /// </summary>
        public int RemainingTicks { get; internal set; } = remainingTicks;

        /// <summary>
        /// Gets the ID of the player that is capturing the zone.
        /// </summary>
        [JsonProperty]
        internal string CapturedById { get; private set; } = capturedBy?.Id!;

        /// <summary>
        /// Gets the ID of the player that is being retaking the zone.
        /// </summary>
        [JsonProperty]
        internal string RetakenById { get; private set; } = retakenBy?.Id!;
    }
}
