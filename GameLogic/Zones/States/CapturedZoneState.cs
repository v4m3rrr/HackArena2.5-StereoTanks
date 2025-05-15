using Newtonsoft.Json;

namespace GameLogic.ZoneStates;

#if !STEREO

/// <summary>
/// Represents a zone that is captured.
/// </summary>
/// <param name="player">The player that captured the zone.</param>
public class CapturedZoneState(Player player) : ZoneState
{
    /// <summary>
    /// Gets or sets the player that is capturing the zone.
    /// </summary>
    [JsonIgnore]
    public Player Player { get; set; } = player;

    /// <summary>
    /// Gets the ID of the player that is capturing the zone.
    /// </summary>
    [JsonProperty]
    internal string PlayerId { get; private set; } = player?.Id!;
}

#endif
