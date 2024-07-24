namespace GameLogic.Networking;

/// <summary>
/// Represents a tank movement direction payload.
/// </summary>
/// <param name="direction">The tank movement direction</param>
public class TankMovementPayload(TankMovement direction) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.TankMovement;

    /// <summary>
    /// Gets the tank movement direction.
    /// </summary>
    public TankMovement Direction { get; } = direction;
}
