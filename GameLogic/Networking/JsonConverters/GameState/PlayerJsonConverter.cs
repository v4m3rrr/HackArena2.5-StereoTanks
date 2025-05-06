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
        var ping = jObject["ping"]!.Value<int>()!;
#if !STEREO
        var color = jObject["color"]!.Value<uint>()!;
        var nickname = jObject["nickname"]!.Value<string>()!;
#endif

        Grid.VisibilityPayload? visibility = default;

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(id))
        {
            var remainingTicksToRegen = jObject["ticksToRegen"]!.Value<int?>();
#if !STEREO
            var score = jObject["score"]!.Value<int>()!;
            var isUsingRadar = jObject["isUsingRadar"]!.Value<bool>();
#endif

            if (context is GameSerializationContext.Spectator)
            {
                visibility = jObject["visibility"]!.ToObject<Grid.VisibilityPayload>(serializer)!;
            }

            return new Player(id, remainingTicksToRegen, visibility?.VisibilityGrid)
            {
                Ping = ping,
#if !STEREO
                Color = color,
                Nickname = nickname,
                Score = score,
                IsUsingRadar = isUsingRadar,
#endif
            };
        }

        return new Player(id)
        {
            Ping = ping,
#if !STEREO
            Color = color,
            Nickname = nickname,
#endif
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Player? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["id"] = value!.Id,
            ["ping"] = value.Ping,
#if !STEREO
            ["color"] = value.Color,
            ["nickname"] = value.Nickname,
#endif
        };

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(value.Id))
        {
            jObject["ticksToRegen"] = value.RemainingTicksToRegen;
#if !STEREO
            jObject["score"] = value.Score;
            jObject["isUsingRadar"] = value.IsUsingRadar;
#endif
        }

        if (context is GameSerializationContext.Spectator)
        {
            var visibilityPayload = new Grid.VisibilityPayload(value.VisibilityGrid!);
            jObject["visibility"] = JToken.FromObject(visibilityPayload, serializer);
        }

        jObject.WriteTo(writer);
    }
}
