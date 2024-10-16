namespace GameLogic.Networking;

/// <summary>
/// Represents a rotation payload.
/// </summary>
public class RotationPayload : IPacketPayload, IActionPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.Rotation;

    /// <inheritdoc/>
    public string? GameStateId { get; init; }

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
    void IActionPayload.ValidateEnums()
    {
        if (this.TankRotation is not null && !Enum.IsDefined(this.TankRotation.Value))
        {
            throw new Exceptions.ConvertEnumFailed<Rotation>(this.TankRotation.Value.ToString());
        }

        if (this.TurretRotation is not null && !Enum.IsDefined(this.TurretRotation.Value))
        {
            throw new Exceptions.ConvertEnumFailed<Rotation>(this.TurretRotation.Value.ToString());
        }
    }
}
