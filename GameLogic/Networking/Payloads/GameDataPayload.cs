namespace GameLogic.Networking;

/// <summary>
/// Represents a game state payload.
/// </summary>
/// <param name="broadcastInterval">The broadcast interval in milliseconds.</param>
/// <param name="playerId">The player id.</param>
/// <param name="seed">The seed of the grid.</param>
public class GameDataPayload(string playerId, int broadcastInterval, int seed) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.GameData;

    /// <summary>
    /// Gets the player id.
    /// </summary>
    public string PlayerId { get; } = playerId;

    /// <summary>
    /// Gets the broadcast interval in milliseconds.
    /// </summary>
    public int BroadcastInterval { get; } = broadcastInterval;

    /// <summary>
    /// Gets the seed of the grid.
    /// </summary>
    public int Seed { get; } = seed;
}
