using GameLogic.Networking.GameEnd;
using Newtonsoft.Json;

namespace GameLogic.Networking;

#if STEREO
/// <summary>
/// Represents a payload for the game end packet.
/// </summary>
/// <param name="Teams">The list of teams.</param>
public record class GameEndPayload(List<Team> Teams) : IPacketPayload
#else
/// <summary>
/// Represents a payload for the game end packet.
/// </summary>
/// <param name="Players">The list of players.</param>
public record class GameEndPayload(List<Player> Players) : IPacketPayload
#endif
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.GameEnded;

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
            new PlayerJsonConverter(context),
        ];
    }
}
