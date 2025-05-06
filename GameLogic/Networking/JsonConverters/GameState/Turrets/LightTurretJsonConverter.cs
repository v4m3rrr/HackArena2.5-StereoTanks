using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Represents a light turret JSON converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class LightTurretJsonConverter(GameSerializationContext context) : JsonConverter<LightTurret>
{
    /// <inheritdoc/>
    public override LightTurret? ReadJson(JsonReader reader, Type objectType, LightTurret? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var direction = JsonConverterUtils.ReadEnum<Direction>(jsonObject["direction"]!);
        var bulletCount = jsonObject["bulletCount"]?.Value<int>();

        if (bulletCount is null)
        {
            // Player perspective for other players
            return new LightTurret(direction);
        }

        var remainingTicksToBullet = jsonObject["ticksToBullet"]?.Value<int?>();
        var remainingTicksToDoubleBullet = jsonObject["ticksToDoubleBullet"]?.Value<int?>();
        return new LightTurret(direction, bulletCount.Value, remainingTicksToBullet, remainingTicksToDoubleBullet);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, LightTurret? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["direction"] = JsonConverterUtils.WriteEnum(value!.Direction, context.EnumSerialization),
        };

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(value.Tank.Owner.Id))
        {
            jObject["bulletCount"] = value.BulletCount;
            jObject["ticksToBullet"] = value.RemainingTicksToBullet;
            jObject["ticksToDoubleBullet"] = value.RemainingTicksToDoubleBullet;
        }

        jObject.WriteTo(writer);
    }
}

#endif
