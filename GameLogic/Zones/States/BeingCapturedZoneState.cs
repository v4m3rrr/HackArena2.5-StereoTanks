using Newtonsoft.Json;

namespace GameLogic.ZoneStates;

#if !STEREO

/// <summary>
/// Represents a zone that is being captured.
/// </summary>
/// <param name="player">The player that is capturing the zone.</param>
/// <param name="remainingTicks">The number of ticks remaining until the zone is captured.</param>
public class BeingCapturedZoneState(Player player, int remainingTicks) : ZoneState, ICaptureState
{
    /// <summary>
    /// Gets or sets the player that is being capturing the zone.
    /// </summary>
    [JsonIgnore]
    public Player Player { get; set; } = player;

    /// <inheritdoc/>
    [JsonIgnore]
    Player ICaptureState.BeingCapturedBy => this.Player;

    /// <inheritdoc/>
    public int RemainingTicks { get; set; } = remainingTicks;

    /// <summary>
    /// Gets the ID of the player that is being capturing the zone.
    /// </summary>
    [JsonProperty]
    internal string PlayerId { get; private set; } = player?.Id!;
}

#endif
