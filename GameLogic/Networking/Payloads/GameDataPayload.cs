namespace GameLogic.Networking;

/// <summary>
/// Represents a game state payload.
/// </summary
/// <param name="playerId">The player id.</param>
/// <param name="dimension">The dimension of the grid.</param>
/// <param name="seed">The seed of the grid.</param>
/// <param name="broadcastInterval">The broadcast interval in milliseconds.</param>
public class GameDataPayload(string playerId, int dimension, int seed, int broadcastInterval) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.GameData;

    /// <summary>
    /// Gets the player id.
    /// </summary>
    public string PlayerId { get; } = playerId;

    /// <summary>
    /// Gets the dimension of the grid.
    /// </summary>
    public int Dimension { get; } = dimension;

    /// <summary>
    /// Gets the seed of the grid.
    /// </summary>
    public int Seed { get; } = seed;

    /// <summary>
    /// Gets the broadcast interval in milliseconds.
    /// </summary>
    public int BroadcastInterval { get; } = broadcastInterval;
}
