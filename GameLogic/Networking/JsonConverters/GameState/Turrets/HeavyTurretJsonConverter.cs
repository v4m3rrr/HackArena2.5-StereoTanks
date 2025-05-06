using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Represents a light turret JSON converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class HeavyTurretJsonConverter(GameSerializationContext context) : JsonConverter<HeavyTurret>
{
    /// <inheritdoc/>
    public override HeavyTurret? ReadJson(JsonReader reader, Type objectType, HeavyTurret? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var direction = JsonConverterUtils.ReadEnum<Direction>(jsonObject["direction"]!);
        var bulletCount = jsonObject["bulletCount"]?.Value<int>();

        if (bulletCount is null)
        {
            // Player perspective for other players
            return new HeavyTurret(direction);
        }

        var remainingTicksToBullet = jsonObject["ticksToBullet"]?.Value<int?>();
        var remainingTicksToLaser = jsonObject["ticksToLaser"]?.Value<int?>();
        return new HeavyTurret(direction, bulletCount.Value, remainingTicksToBullet, remainingTicksToLaser);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, HeavyTurret? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["direction"] = JsonConverterUtils.WriteEnum(value!.Direction, context.EnumSerialization),
        };

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(value.Tank.Owner.Id))
        {
            jObject["bulletCount"] = value.BulletCount;
            jObject["ticksToBullet"] = value.RemainingTicksToBullet;
            jObject["ticksToLaser"] = value.RemainingTicksToLaser;
        }

        jObject.WriteTo(writer);
    }
}

#endif
