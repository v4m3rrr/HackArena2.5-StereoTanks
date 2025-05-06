using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Represents a tank json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class LightTankJsonConverter(GameSerializationContext context) : JsonConverter<LightTank>
{
    /// <inheritdoc/>
    public override LightTank? ReadJson(JsonReader reader, Type objectType, LightTank? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        int x = jsonObject["x"]?.Value<int>() ?? -1;
        int y = jsonObject["y"]?.Value<int>() ?? -1;

        var ownerId = jsonObject["ownerId"]!.Value<string>()!;
        var direction = JsonConverterUtils.ReadEnum<Direction>(jsonObject["direction"]!);
        var turret = jsonObject["turret"]!.ToObject<LightTurret>(serializer)!;

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(ownerId))
        {
            var health = jsonObject["health"]!.Value<int?>();
            var ticksToRadar = jsonObject["ticksToRadar"]!.Value<int?>();
            var isUsingRadar = jsonObject["isUsingRadar"]!.Value<bool>();
            return new LightTank(x, y, ownerId, health ?? 0, direction, turret, ticksToRadar)
            {
                IsUsingRadar = isUsingRadar,
            };
        }

        return new LightTank(x, y, ownerId, direction, turret);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, LightTank? value, JsonSerializer serializer)
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
            jObject["ticksToRadar"] = value.RemainingTicksToRadar;
            jObject["isUsingRadar"] = value.IsUsingRadar;
        }

        jObject.WriteTo(writer);
    }
}

#endif
