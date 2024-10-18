namespace GameLogic.Networking;

/// <summary>
/// Represents a pass payload.
/// </summary>
public class PassPayload : IPacketPayload, IActionPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.Pass;

    /// <inheritdoc/>
    public string? GameStateId { get; init; }

    /// <inheritdoc/>
    public void ValidateEnums()
    {
    }
}
