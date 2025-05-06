namespace GameLogic.Networking;

/// <summary>
/// Represents a pass payload.
/// </summary>
public class PassPayload : ActionPayload
{
    /// <inheritdoc/>
    public override PacketType Type => PacketType.Pass;
}
