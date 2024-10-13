using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a tank json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class TankJsonConverter(GameSerializationContext context) : JsonConverter<Tank>
{
    /// <inheritdoc/>
    public override Tank? ReadJson(JsonReader reader, Type objectType, Tank? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        int x = jsonObject["x"]?.Value<int>() ?? -1;
        int y = jsonObject["y"]?.Value<int>() ?? -1;

        var ownerId = jsonObject["ownerId"]!.Value<string>()!;
        var direction = JsonConverterUtils.ReadEnum<Direction>(jsonObject["direction"]!);

        Turret turret = jsonObject["turret"]!.ToObject<Turret>(serializer)!;

        Tank? tank = null;
        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(ownerId))
        {
            var health = jsonObject["health"]!.Value<int?>();
            var secondaryItemType = (SecondaryItemType?)jsonObject["secondaryItem"]?.Value<int?>()!;

            tank = new Tank(x, y, ownerId, health ?? 0, direction, turret, secondaryItemType);
        }
        else if (context is GameSerializationContext.Player)
        {
            tank = new Tank(x, y, ownerId, direction, turret);
        }

        return tank;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Tank? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["ownerId"] = value!.OwnerId,
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
            jObject["secondaryItem"] = (int?)value.SecondaryItemType;
        }

        jObject.WriteTo(writer);
    }
}
