using GameLogic.Networking.Exceptions;

namespace GameLogic.Networking;

/// <summary>
/// Represents a movement payload.
/// </summary>
/// <param name="direction">The direction of the movement.</param>
public class MovementPayload(MovementDirection direction) : ActionPayload
{
    /// <inheritdoc/>
    public override PacketType Type => PacketType.Movement;

    /// <summary>
    /// Gets the direction of the movement.
    /// </summary>
    public MovementDirection Direction { get; } = direction;

    /// <inheritdoc/>
    internal override void ValidateEnums()
    {
        if (!Enum.IsDefined(this.Direction))
        {
            throw new PayloadEnumValidationError<MovementDirection>(this.Direction);
        }
    }
}
