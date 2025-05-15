using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Represents a light turret JSON converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class HeavyTurretJsonConverter(GameSerializationContext context)
    : JsonConverter<HeavyTurret>
{
    /// <inheritdoc/>
    public override HeavyTurret? ReadJson(JsonReader reader, Type objectType, HeavyTurret? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var direction = JsonConverterUtils.ReadEnum<Direction>(jObject["direction"]!, context.EnumSerialization);
        var turret = new HeavyTurret(direction);

        if (context is GameSerializationContext.Spectator || jObject["bulletCount"] is not null)
        {
            turret.Bullet = new BulletAbility(null!)
            {
                Count = jObject["bulletCount"]!.Value<int>(),
                RemainingRegenerationTicks = jObject["ticksToBullet"]!.Value<int?>(),
            };

            turret.Laser = new LaserAbility(null!)
            {
                RemainingRegenerationTicks = jObject["ticksToLaser"]!.Value<int?>(),
            };

            turret.HealingBullet = new HealingBulletAbility(null!)
            {
                RemainingRegenerationTicks = jObject["ticksToHealingBullet"]!.Value<int?>(),
            };

            turret.StunBullet = new StunBulletAbility(null!)
            {
                RemainingRegenerationTicks = jObject["ticksToStunBullet"]!.Value<int?>(),
            };
        }

        return turret;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, HeavyTurret? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["direction"] = JsonConverterUtils.WriteEnum(value!.Direction, context.EnumSerialization),
        };

        var isSpectator = context is GameSerializationContext.Spectator;
        var isOwner = !isSpectator && context.IsPlayerWithId(value.Tank.Owner.Id);
        var isTeammate = !isOwner && context.IsTeammate(value.Tank.Owner.Id);

        if (isSpectator || isOwner || isTeammate)
        {
            jObject["bulletCount"] = value.Bullet!.Count;
            jObject["ticksToBullet"] = value.Bullet.RemainingRegenerationTicks;
            jObject["ticksToHealingBullet"] = value.HealingBullet!.RemainingRegenerationTicks;
            jObject["ticksToStunBullet"] = value.StunBullet!.RemainingRegenerationTicks;
            jObject["ticksToLaser"] = value.Laser!.RemainingRegenerationTicks;
        }

        jObject.WriteTo(writer);
    }
}

#endif
