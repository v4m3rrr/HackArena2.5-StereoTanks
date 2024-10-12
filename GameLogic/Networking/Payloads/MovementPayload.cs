namespace GameLogic.Networking;

/// <summary>
/// Represents a movement payload.
/// </summary>
/// <param name="direction">The tank movement direction.</param>
public class MovementPayload(MovementDirection direction) : IPacketPayload, IActionPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.Movement;

    /// <inheritdoc/>
    public string? GameStateId { get; init; }

    /// <summary>
    /// Gets the tank movement direction.
    /// </summary>
    public MovementDirection Direction { get; } = direction;
}
