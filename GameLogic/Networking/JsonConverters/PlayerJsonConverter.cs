using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a player json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class PlayerJsonConverter(SerializationContext context) : JsonConverter<Player>
{
    private readonly SerializationContext context = context;

    /// <inheritdoc/>
    public override Player? ReadJson(JsonReader reader, Type objectType, Player? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var id = jObject["id"]!.Value<string>()!;
        var nickname = jObject["nickname"]!.Value<string>()!;
        var color = jObject["color"]!.Value<uint>()!;
        var ping = jObject["ping"]!.Value<int>()!;

        if (this.context is SerializationContext.Spectator || this.context.IsPlayerWithId(id))
        {
            var score = jObject["score"]!.Value<int>()!;

            return new Player(id, nickname, color)
            {
                Ping = ping,
                Score = score,
            };
        }

        return new Player(id, nickname, color)
        {
            Ping = ping,
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Player? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["id"] = value!.Id,
            ["nickname"] = value.Nickname,
            ["color"] = value.Color,
            ["ping"] = value.Ping,
        };

        if (this.context is SerializationContext.Spectator || this.context.IsPlayerWithId(value.Id))
        {
            jObject["score"] = value.Score;
        }

        jObject.WriteTo(writer);
    }
}
