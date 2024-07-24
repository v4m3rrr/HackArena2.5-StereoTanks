namespace GameLogic.Networking;

/// <summary>
/// Represents a game state payload.
/// </summary>
/// <param name="id">The game id.</param>
/// <param name="joinCode">The join code.</param>
/// <param name="broadcastInterval">The broadcast interval.</param>
public class GameStatePayload(string id, string joinCode, float broadcastInterval) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.GameData;

    /// <summary>
    /// Gets the game id.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Gets the join code.
    /// </summary>
    public string JoinCode { get; } = joinCode;

    /// <summary>
    /// Gets the broadcast interval.
    /// </summary>
    public float BroadcastInterval { get; } = broadcastInterval;
}
