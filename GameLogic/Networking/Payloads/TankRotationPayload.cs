namespace GameLogic.Networking;

/// <summary>
/// Represents a tank rotation payload.
/// </summary>
public class TankRotationPayload : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.TankRotation;

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
}
