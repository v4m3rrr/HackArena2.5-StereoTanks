using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a turret json converter.
/// </summary>
/// <param name="context">The serializaton context.</param>
internal class TurretJsonConverter(SerializationContext context) : JsonConverter<Turret>
{
    private readonly SerializationContext context = context;

    /// <inheritdoc/>
    public override Turret? ReadJson(JsonReader reader, Type objectType, Turret? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        var direction = (Direction)jsonObject["direction"]!.Value<int>()!;

        var bulletCount = jsonObject["bulletCount"]?.Value<int>() ?? null;
        var bulletRegenProgress = jsonObject["bulletRegenProgress"]?.Value<float?>() ?? null;

        if (bulletCount is null)
        {
            // Player perspective for other players
            return new Turret(direction);
        }

        return new Turret(direction, bulletCount.Value, bulletRegenProgress);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Turret? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["direction"] = (int)value!.Direction,
        };

        if (this.context is SerializationContext.Spectator || this.context.IsPlayerWithId(value.Tank.Owner.Id))
        {
            jObject["bulletCount"] = value.BulletCount;
            jObject["bulletRegenProgress"] = value.BulletRegenProgress;
        }

        jObject.WriteTo(writer);
    }
}
