namespace GameLogic.Networking;

/// <summary>
/// Represents a tank movement direction payload.
/// </summary>
/// <param name="direction">The tank movement direction.</param>
public class TankMovementPayload(TankMovement direction) : IPacketPayload, IActionPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.TankMovement;

    /// <inheritdoc/>
    public string? GameStateId { get; init; }

    /// <summary>
    /// Gets the tank movement direction.
    /// </summary>
    public TankMovement Direction { get; } = direction;
}
