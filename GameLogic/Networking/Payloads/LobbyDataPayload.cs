using GameLogic.Networking.LobbyData;
using Newtonsoft.Json;

namespace GameLogic.Networking;

#if STEREO
/// <summary>
/// Represents a game state payload.
/// </summary>
/// <param name="PlayerId">The player id.</param>
/// <param name="TeamName">The team name.</param>
/// <param name="Teams">The list of teams.</param>
/// <param name="ServerSettings">The server settings.</param>
public record class LobbyDataPayload(
    string? PlayerId,
    string? TeamName,
    List<Team> Teams,
    ServerSettings ServerSettings) : IPacketPayload
#else
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
#endif
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.LobbyData;

    /// <summary>
    /// Gets the converters to use during
    /// serialization and deserialization
    /// with default context.
    /// </summary>
    /// <returns>
    /// The list of converters to use during
    /// serialization and deserialization.
    /// </returns>
    public static List<JsonConverter> GetConverters()
    {
        return GetConverters(SerializationContext.Default);
    }

    /// <summary>
    /// Gets the converters to use during
    /// serialization and deserialization.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <returns>
    /// The list of converters to use during
    /// serialization and deserialization.
    /// </returns>
    public static List<JsonConverter> GetConverters(SerializationContext context)
    {
        return [
#if STEREO
            new TeamJsonConverter(),
#endif
            new PlayerJsonConverter(context)
        ];
    }
}
