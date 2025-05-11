using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#if STEREO

/// <summary>
/// Converts <see langword="HeavyTank"/> JSON snapshot with per-player visibility.
/// </summary>
/// <param name="context">The serialization context with enum format and player scope.</param>
internal class HeavyTankJsonConverter(GameSerializationContext context)
    : JsonConverter<HeavyTank>
{
    /// <inheritdoc/>
    public override HeavyTank? ReadJson(
        JsonReader reader,
        Type objectType,
        HeavyTank? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        int x = jObject["x"]?.Value<int>() ?? -1;
        int y = jObject["y"]?.Value<int>() ?? -1;
        string ownerId = jObject["ownerId"]!.Value<string>()!;
        var direction = JsonConverterUtils.ReadEnum<Direction>(jObject["direction"]!, context.EnumSerialization);

        var turretToken = jObject["turret"]!;
        var turret = turretToken.ToObject<HeavyTurret>(serializer)!;

        var tank = new HeavyTank(x, y, direction, new Player(ownerId))
        {
            Turret = turret,
        };

        var isSpectator = context is GameSerializationContext.Spectator;
        var isOwner = !isSpectator && context.IsPlayerWithId(ownerId);
        var isTeammate = !isOwner && context.IsTeammate(ownerId);

        if (isSpectator || isOwner)
        {
            tank.Health = jObject["health"]!.Value<int>();

            tank.Mine = new MineAbility(null!)
            {
                RemainingRegenerationTicks = jObject["ticksToMine"]!.Value<int?>(),
            };
        }

        if (isSpectator || isOwner || isTeammate)
        {
            tank.VisibilityGrid = jObject["visibility"]?.ToObject<VisibilityPayload>(serializer)!.Grid;
        }

        return tank;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, HeavyTank? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["ownerId"] = value!.OwnerId,
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
            jObject["ticksToMine"] = value.Mine!.RemainingRegenerationTicks;
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
