using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a laser json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class MineJsonConverter(GameSerializationContext context) : JsonConverter<Mine>
{
    /// <inheritdoc/>
    public override Mine? ReadJson(JsonReader reader, Type objectType, Mine? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var id = jObject["id"]!.Value<int>()!;
        var x = jObject["x"]!.Value<int>();
        var y = jObject["y"]!.Value<int>();
        var explosionRemainingTicks = jObject["explosionRemainingTicks"]?.Value<int?>();

        if (context is GameSerializationContext.Player)
        {
            return new Mine(id, x, y)
            {
                ExplosionRemainingTicks = explosionRemainingTicks,
            };
        }

        var damage = jObject["damage"]!.Value<int>();
        var layerId = jObject["layerId"]!.Value<string>()!;

        return new Mine(id, x, y, damage, layerId)
        {
            ExplosionRemainingTicks = explosionRemainingTicks,
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Mine? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["id"] = value!.Id,
            ["explosionRemainingTicks"] = value.ExplosionRemainingTicks,
        };

        if (context is GameSerializationContext.Spectator)
        {
            jObject["x"] = value.X;
            jObject["y"] = value.Y;
            jObject["damage"] = value.Damage;
            jObject["layerId"] = value.LayerId;
        }

        jObject.WriteTo(writer);
    }
}
