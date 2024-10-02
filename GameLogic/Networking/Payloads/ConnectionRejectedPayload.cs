namespace GameLogic.Networking;

/// <summary>
/// Represents a connection rejected payload.
/// </summary>
/// <param name="Reason">The reason for the rejection.</param>
public record class ConnectionRejectedPayload(string Reason) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.ConnectionRejected;
}
