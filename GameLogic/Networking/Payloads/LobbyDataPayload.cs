using GameLogic.Networking.LobbyData;
using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a game state payload.
/// </summary>
/// <param name="PlayerId">The player id.</param>
/// <param name="Players">The list of players.</param>
/// <param name="GridDimension">The dimension of the grid.</param>
/// <param name="Seed">The seed of the grid.</param>
/// <param name="BroadcastInterval">The broadcast interval in milliseconds.</param>
public record class LobbyDataPayload(
    string? PlayerId,
    List<Player> Players,
    int GridDimension,
    int Seed,
    int BroadcastInterval) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.LobbyData;

    /// <summary>
    /// Gets the converters to use during
    /// serialization and deserialization.
    /// </summary>
    /// <returns>
    /// The list of converters to use during
    /// serialization and deserialization.
    /// </returns>
    public static List<JsonConverter> GetConverters()
    {
        return [new PlayerJsonConverter()];
    }
}
