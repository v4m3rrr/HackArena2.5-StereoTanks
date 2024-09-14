using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.LobbyData;

/// <summary>
/// Represents a player json converter.
/// </summary>
internal class PlayerJsonConverter : JsonConverter<Player>
{
    /// <inheritdoc/>
    public override Player? ReadJson(JsonReader reader, Type objectType, Player? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var id = jObject["id"]!.Value<string>()!;
        var nickname = jObject["nickname"]!.Value<string>()!;
        var color = jObject["color"]!.Value<uint>()!;

        return new Player(id, nickname, color);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Player? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["id"] = value!.Id,
            ["nickname"] = value.Nickname,
            ["color"] = value.Color,
        };

        jObject.WriteTo(writer);
    }
}
