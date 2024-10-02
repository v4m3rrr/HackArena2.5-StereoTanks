namespace GameLogic.Networking;

/// <summary>
/// Represents a tank shoot payload.
/// </summary>
public class TankShootPayload : IPacketPayload, IActionPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.TankShoot;

    /// <inheritdoc/>
    public string? GameStateId { get; init; }
}
