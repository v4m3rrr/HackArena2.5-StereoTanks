namespace GameLogic.Networking;

#if STEREO

/// <summary>
/// Represents a payload for the capture zone action.
/// </summary>
public class CaptureZonePayload : ActionPayload
{
    /// <inheritdoc/>
    public override PacketType Type => PacketType.CaptureZone;
}

#endif
