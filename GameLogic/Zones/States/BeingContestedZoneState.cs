using Newtonsoft.Json;

namespace GameLogic.ZoneStates;

#if !STEREO

/// <summary>
/// Represents a zone that is being contested.
/// </summary>
/// <param name="capturedBy">The player that captured the zone, if the zone was captured.</param>
public class BeingContestedZoneState(Player? capturedBy) : ZoneState
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

#endif
