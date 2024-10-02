using GameLogic.Networking.LobbyData;
using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a game state payload.
/// </summary>
/// <param name="PlayerId">The player id.</param>
/// <param name="Players">The list of players.</param>
/// <param name="ServerSettings">The server settings.</param>
public record class LobbyDataPayload(
    string? PlayerId,
    List<Player> Players,
    ServerSettings ServerSettings) : IPacketPayload
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
