using GameLogic.Networking.GameEnd;
using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a payload for the game end packet.
/// </summary>
/// <param name="Players">The list of players.</param>
public record class GameEndPayload(List<Player> Players) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.GameEnd;

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
