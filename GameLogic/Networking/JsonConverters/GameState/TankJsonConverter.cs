using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a tank json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class TankJsonConverter(GameSerializationContext context) : JsonConverter<Tank>
{
#if STEREO
    private readonly LightTankJsonConverter lightTankConverter = new(context);
    private readonly HeavyTankJsonConverter heavyTankConverter = new(context);
#endif

    /// <inheritdoc/>
    public override Tank? ReadJson(JsonReader reader, Type objectType, Tank? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

#if STEREO

        var type = JsonConverterUtils.ReadEnum<TankType>(jObject["type"]!);
        return type switch
        {
            TankType.Light => this.lightTankConverter.ReadJson(jObject.CreateReader(), typeof(LightTank), null, false, serializer),
            TankType.Heavy => this.heavyTankConverter.ReadJson(jObject.CreateReader(), typeof(HeavyTank), null, false, serializer),
            _ => throw new ArgumentOutOfRangeException(nameof(reader), type, null),
        };

#else

        int x = jObject["x"]?.Value<int>() ?? -1;
        int y = jObject["y"]?.Value<int>() ?? -1;
        string ownerId = jObject["ownerId"]!.Value<string>()!;
        var direction = JsonConverterUtils.ReadEnum<Direction>(jObject["direction"]!, context.EnumSerialization);
        var turret = jObject["turret"]!.ToObject<Turret>(serializer)!;

        var playerStub = new Player(ownerId)
        {
            Nickname = string.Empty,
            Color = default,
        };

        var tank = new Tank(x, y, direction, playerStub)
        {
            Turret = turret,
        };

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(ownerId))
        {
            tank.Health = jObject["health"]!.Value<int>();

            tank.SecondaryItemType = jObject["secondaryItem"]!.Type is not JTokenType.Null
                ? JsonConverterUtils.ReadEnum<SecondaryItemType>(jObject["secondaryItem"]!)
                : null;

            tank.Mine = new MineAbility(null!);
        }

        return tank;

#endif
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Tank? value, JsonSerializer serializer)
    {
#if STEREO

        switch (value)
        {
            case LightTank lightTank:
                this.lightTankConverter.WriteJson(writer, lightTank, serializer);
                break;
            case HeavyTank heavyTank:
                this.heavyTankConverter.WriteJson(writer, heavyTank, serializer);
                break;
            default:
                throw new JsonSerializationException($"Unknown Tank subtype: {value?.GetType().Name}");
        }

#else

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
            jObject["secondaryItem"] = value.SecondaryItemType is not null
                ? JsonConverterUtils.WriteEnum(value.SecondaryItemType.Value, context.EnumSerialization)
                : null;
        }

        jObject.WriteTo(writer);

#endif
    }
}
