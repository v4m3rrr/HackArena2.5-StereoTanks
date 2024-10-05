namespace GameLogic.Networking;

/// <summary>
/// Represents a response pass payload.
/// </summary>
public class ResponsePassPayload : IPacketPayload, IActionPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.ResponsePass;

    /// <inheritdoc/>
    public string? GameStateId { get; init; }
}
