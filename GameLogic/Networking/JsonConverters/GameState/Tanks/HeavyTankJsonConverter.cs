using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Represents a heavy tank JSON converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class HeavyTankJsonConverter(GameSerializationContext context) : JsonConverter<HeavyTank>
{
    /// <inheritdoc/>
    public override HeavyTank? ReadJson(JsonReader reader, Type objectType, HeavyTank? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        int x = jsonObject["x"]?.Value<int>() ?? -1;
        int y = jsonObject["y"]?.Value<int>() ?? -1;

        var ownerId = jsonObject["ownerId"]!.Value<string>()!;
        var direction = JsonConverterUtils.ReadEnum<Direction>(jsonObject["direction"]!);
        var turret = jsonObject["turret"]!.ToObject<HeavyTurret>(serializer)!;

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(ownerId))
        {
            var health = jsonObject["health"]!.Value<int?>();
            var ticksToMine = jsonObject["ticksToMine"]?.Value<int?>();
            return new HeavyTank(x, y, ownerId, health ?? 0, direction, turret, ticksToMine);
        }

        return new HeavyTank(x, y, ownerId, direction, turret);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, HeavyTank? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["ownerId"] = value!.OwnerId,
            ["type"] = JsonConverterUtils.WriteEnum(value.Type, context.EnumSerialization),
            ["direction"] = JsonConverterUtils.WriteEnum(value.Direction, context.EnumSerialization),
            ["turret"] = JObject.FromObject(value.Turret, serializer),
        };

        if (context is GameSerializationContext.Spectator)
        {
            jObject["x"] = value.X;
            jObject["y"] = value.Y;
        }

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(value.Owner.Id))
        {
            jObject["health"] = value.Health;
            jObject["ticksToMine"] = value.RemainingTicksToMine;
        }

        jObject.WriteTo(writer);
    }
}

#endif
