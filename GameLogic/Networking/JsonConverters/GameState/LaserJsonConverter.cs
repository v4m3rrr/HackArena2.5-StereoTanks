using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a laser json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class LaserJsonConverter(GameSerializationContext context) : JsonConverter<Laser>
{
    /// <inheritdoc/>
    public override Laser? ReadJson(JsonReader reader, Type objectType, Laser? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var id = jObject["id"]!.Value<int>()!;
        var x = jObject["x"]!.Value<int>();
        var y = jObject["y"]!.Value<int>();
        var orientation = (Orientation)jObject["orientation"]!.Value<int>()!;

        if (context is GameSerializationContext.Player)
        {
            return new Laser(id, x, y, orientation);
        }

        var damage = jObject["damage"]!.Value<int>();
        var shooterId = jObject["shooterId"]!.Value<string>()!;

        return new Laser(id, x, y, orientation, damage, shooterId);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Laser? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["id"] = value!.Id,
            ["orientation"] = (int)value.Orientation,
        };

        if (context is GameSerializationContext.Spectator)
        {
            jObject["x"] = value.X;
            jObject["y"] = value.Y;
            jObject["damage"] = value.Damage;
            jObject["shooterId"] = value.ShooterId;
        }

        jObject.WriteTo(writer);
    }
}
