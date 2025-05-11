using Newtonsoft.Json;

namespace GameLogic.ZoneStates;

/// <summary>
/// Represents a zone that is being retaken.
/// </summary>
/// <param name="capturedBy">The player that captured the zone.</param>
/// <param name="retakenBy">The player that is retaking the zone.</param>
/// <param name="remainingTicks">The number of ticks remaining until the zone is retaken.</param>
public class BeingRetakenZoneState(Player capturedBy, Player retakenBy, int remainingTicks) : ZoneState, ICaptureState
{
    /// <summary>
    /// Gets or sets the player that is capturing the zone.
    /// </summary>
    [JsonIgnore]
    public Player CapturedBy { get; set; } = capturedBy;

    /// <summary>
    /// Gets or sets the player that is being retaking the zone.
    /// </summary>
    [JsonIgnore]
    public Player RetakenBy { get; set; } = retakenBy;

    /// <inheritdoc/>
    [JsonIgnore]
    Player ICaptureState.BeingCapturedBy => this.RetakenBy;

    /// <summary>
    /// Gets or sets the number of ticks remaining until the zone is retaken.
    /// </summary>
    public int RemainingTicks { get; set; } = remainingTicks;

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
