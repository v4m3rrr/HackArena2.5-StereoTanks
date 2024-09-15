using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a player json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class PlayerJsonConverter(GameSerializationContext context) : JsonConverter<Player>
{
    /// <inheritdoc/>
    public override Player? ReadJson(JsonReader reader, Type objectType, Player? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var id = jObject["id"]!.Value<string>()!;
        var nickname = jObject["nickname"]!.Value<string>()!;
        var color = jObject["color"]!.Value<uint>()!;
        var ping = jObject["ping"]!.Value<int>()!;

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(id))
        {
            var score = jObject["score"]!.Value<int>()!;
            var regenProgress = jObject["regenProgress"]?.Value<float?>();

            return new Player(id, nickname, color, regenProgress)
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

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(value.Id))
        {
            jObject["score"] = value.Score;
            jObject["regenProgress"] = value.RegenProgress;
        }

        jObject.WriteTo(writer);
    }
}
