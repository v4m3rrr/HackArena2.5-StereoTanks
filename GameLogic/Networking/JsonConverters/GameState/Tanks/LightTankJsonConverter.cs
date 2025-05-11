using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Converts <see cref="LightTank"/> JSON using snapshot + UpdateFrom strategy.
/// </summary>
/// <param name="context">The converter context with factory and player mapping.</param>
internal class LightTankJsonConverter(GameSerializationContext context)
    : JsonConverter<LightTank>
{
    /// <inheritdoc/>
    public override LightTank? ReadJson(JsonReader reader, Type objectType, LightTank? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        int x = jObject["x"]?.Value<int>() ?? -1;
        int y = jObject["y"]?.Value<int>() ?? -1;
        string ownerId = jObject["ownerId"]!.Value<string>()!;
        var direction = JsonConverterUtils.ReadEnum<Direction>(jObject["direction"]!, context.EnumSerialization);
        var turret = jObject["turret"]!.ToObject<LightTurret>(serializer)!;

        var tank = new LightTank(x, y, direction, new Player(ownerId))
        {
            Turret = turret,
        };

        var isSpectator = context is GameSerializationContext.Spectator;
        var isOwner = !isSpectator && context.IsPlayerWithId(ownerId);
        var isTeammate = !isOwner && context.IsTeammate(ownerId);

        if (isSpectator || isOwner)
        {
            tank.Health = jObject["health"]!.Value<int>();

            tank.Radar = new RadarAbility(null!)
            {
                RemainingRegenerationTicks = jObject["ticksToRadar"]!.Value<int?>(),
                IsActive = jObject["isUsingRadar"]!.Value<bool>(),
            };
        }

        if (isSpectator || isOwner || isTeammate)
        {
            tank.VisibilityGrid = jObject["visibility"]?.ToObject<VisibilityPayload>(serializer)!.Grid;
        }

        return tank;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, LightTank? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var jObject = new JObject
        {
            ["ownerId"] = value.OwnerId,
            ["type"] = JsonConverterUtils.WriteEnum(value.Type, context.EnumSerialization),
            ["direction"] = JsonConverterUtils.WriteEnum(value.Direction, context.EnumSerialization),
            ["turret"] = JObject.FromObject(value.Turret, serializer),
        };

        var isSpectator = context is GameSerializationContext.Spectator;
        var isOwner = !isSpectator && context.IsPlayerWithId(value.OwnerId);
        var isTeammate = !isOwner && context.IsTeammate(value.OwnerId);

        if (isSpectator)
        {
            jObject["x"] = value.X;
            jObject["y"] = value.Y;
        }

        if (isSpectator || isOwner)
        {
            jObject["health"] = value.Health;
            jObject["ticksToRadar"] = value.Radar!.RemainingRegenerationTicks;
            jObject["isUsingRadar"] = value.Radar.IsActive;
        }

        if (isSpectator || isOwner || isTeammate)
        {
            var visibilityPayload = new VisibilityPayload(value.VisibilityGrid!);
            jObject["visibility"] = JToken.FromObject(visibilityPayload, serializer);
        }

        jObject.WriteTo(writer);
    }
}

#endif
