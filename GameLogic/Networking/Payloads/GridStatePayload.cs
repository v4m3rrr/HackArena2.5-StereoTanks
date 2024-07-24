namespace GameLogic.Networking;

/// <summary>
/// Represents a grid state payload.
/// </summary>
public class GridStatePayload : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.GridData;

    /// <summary>
    /// Gets the wall grid.
    /// </summary>
    public Wall?[,] WallGrid { get; init; } = new Wall?[0, 0];

    /// <summary>
    /// Gets the tanks.
    /// </summary>
    public List<Tank> Tanks { get; init; } = new();

    /// <summary>
    /// Gets the bullets.
    /// </summary>
    public List<Bullet> Bullets { get; init; } = new();
}