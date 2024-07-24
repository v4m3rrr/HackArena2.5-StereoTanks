namespace GameLogic.Networking;

/// <summary>
/// Represents an empty payload.
/// </summary>
/// <remarks>
/// This class is used to represent an empty payload with specified packet type,
/// which is useful for packets that do not have any payload.
/// </remarks>
public class EmptyPayload : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type { get; init; }
}
