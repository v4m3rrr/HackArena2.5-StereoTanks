namespace GameLogic.Networking;

/// <summary>
/// Represents a tank shoot payload.
/// </summary>
public class TankShootPayload : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.TankShoot;
}