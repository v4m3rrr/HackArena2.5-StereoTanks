using GameLogic.Networking.GoToElements;

namespace GameLogic.Networking;

#if STEREO

/// <summary>
/// Represents a payload for going to a specific position.
/// </summary>
public class GoToPayload(int x, int y) : ActionPayload
{
    /// <inheritdoc/>
    public override PacketType Type => PacketType.GoTo;

    /// <summary>
    /// Gets the turret rotation of the tank.
    /// </summary>
    public Rotation? TurretRotation { get; init; }

    /// <summary>
    /// Gets the x coordinate of the target position.
    /// </summary>
    public int X { get; } = x;

    /// <summary>
    /// Gets the y coordinate of the target position.
    /// </summary>
    public int Y { get; } = y;

    /// <summary>
    /// Gets the costs associated with the pathfinding.
    /// </summary>
    public Costs Costs { get; init; } = new();

    /// <summary>
    /// Gets the penalties associated with the pathfinding.
    /// </summary>
    public Penalties Penalties { get; init; } = new();
}

#endif
