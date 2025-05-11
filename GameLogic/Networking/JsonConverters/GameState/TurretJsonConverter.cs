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
        var jObject = JObject.Load(reader);
        var direction = JsonConverterUtils.ReadEnum<Direction>(jObject["direction"]!, context.EnumSerialization);
        var turret = new Turret(direction);

        if (context is GameSerializationContext.Spectator || jObject["bulletCount"] is not null)
        {
            turret.Bullet = new BulletAbility(null!)
            {
                Count = jObject["bulletCount"]!.Value<int>(),
                RemainingRegenerationTicks = jObject["ticksToBullet"]!.Value<int?>(),
            };

            turret.DoubleBullet = new DoubleBulletAbility(null!);
            turret.Laser = new LaserAbility(null!);
        }

        return turret;
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
            jObject["bulletCount"] = value.Bullet!.Count;
            jObject["ticksToBullet"] = value.Bullet.RemainingRegenerationTicks;
        }

        jObject.WriteTo(writer);
    }
}

#endif
