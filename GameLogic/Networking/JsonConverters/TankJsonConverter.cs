using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a tank json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class TankJsonConverter(SerializationContext context) : JsonConverter<Tank>
{
    private readonly SerializationContext context = context;

    /// <inheritdoc/>
    public override Tank? ReadJson(JsonReader reader, Type objectType, Tank? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        int x = jsonObject["x"]?.Value<int>() ?? -1;
        int y = jsonObject["y"]?.Value<int>() ?? -1;

        var ownerId = jsonObject["ownerId"]!.Value<string>()!;
        var direction = (Direction)jsonObject["direction"]!.Value<int>()!;

        Turret turret = jsonObject["turret"]!.ToObject<Turret>(serializer)!;

        Tank? tank = null;
        if (this.context is SerializationContext.Spectator || this.context.IsPlayerWithId(ownerId))
        {
            var health = jsonObject["health"]!.Value<int?>();
            var regenProgress = jsonObject["regenProgress"]!.Value<float?>();
            tank = new Tank(x, y, ownerId, health ?? 0, regenProgress, direction, turret);
        }
        else if (this.context is SerializationContext.Player)
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
            ["direction"] = (int)value.Direction,
            ["turret"] = JObject.FromObject(value.Turret, serializer),
        };

        if (this.context is SerializationContext.Spectator)
        {
            jObject["x"] = value.X;
            jObject["y"] = value.Y;
        }

        if (this.context is SerializationContext.Spectator || this.context.IsPlayerWithId(value.Owner.Id))
        {
            jObject["health"] = value.Health;
            jObject["regenProgress"] = value.RegenProgress;
        }

        jObject.WriteTo(writer);
    }
}
