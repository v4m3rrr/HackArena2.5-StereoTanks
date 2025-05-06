using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if !STEREO

/// <summary>
/// Represents a turret json converter.
/// </summary>
/// <param name="context">The serializaton context.</param>
internal class TurretJsonConverter(GameSerializationContext context) : JsonConverter<Turret>
{
    /// <inheritdoc/>
    public override Turret? ReadJson(JsonReader reader, Type objectType, Turret? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var direction = JsonConverterUtils.ReadEnum<Direction>(jsonObject["direction"]!);
        var bulletCount = jsonObject["bulletCount"]?.Value<int>();
        var remainingTicksToBullet = jsonObject["ticksToBullet"]?.Value<int?>();

        if (bulletCount is null)
        {
            // Player perspective for other players
            return new Turret(direction);
        }

        return new Turret(direction, bulletCount.Value, remainingTicksToBullet);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Turret? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["direction"] = JsonConverterUtils.WriteEnum(value!.Direction, context.EnumSerialization),
        };

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(value.Tank.Owner.Id))
        {
            jObject["bulletCount"] = value.BulletCount;
            jObject["ticksToBullet"] = value.RemainingTicksToBullet;
        }

        jObject.WriteTo(writer);
    }
}

#endif
