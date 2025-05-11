using GameLogic.Networking.Exceptions;

namespace GameLogic.Networking;

/// <summary>
/// Represents a rotation payload.
/// </summary>
public class RotationPayload : ActionPayload
{
    /// <inheritdoc/>
    public override PacketType Type => PacketType.Rotation;

    /// <summary>
    /// Gets the tank rotation.
    /// </summary>
    /// <remarks>
    /// If the value is <see langword="null"/>,
    /// the tank rotation is not changed.
    /// </remarks>
    public Rotation? TankRotation { get; init; }

    /// <summary>
    /// Gets the turret rotation.
    /// </summary>
    /// <remarks>
    /// If the value is <see langword="null"/>,
    /// the turret rotation is not changed.
    /// </remarks>
    public Rotation? TurretRotation { get; init; }

    /// <inheritdoc/>
    internal override void ValidateEnums()
    {
        if (this.TankRotation is not null && !Enum.IsDefined(this.TankRotation.Value))
        {
            throw new PayloadEnumValidationError<Rotation>(this.TankRotation.Value);
        }

        if (this.TurretRotation is not null && !Enum.IsDefined(this.TurretRotation.Value))
        {
            throw new PayloadEnumValidationError<Rotation>(this.TurretRotation.Value);
        }
    }
}
