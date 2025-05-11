using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a player json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class PlayerJsonConverter(GameSerializationContext context) : JsonConverter<Player>
{
    /// <inheritdoc/>
    public override Player? ReadJson(JsonReader reader, Type objectType, Player? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var id = jObject["id"]!.Value<string>()!;
        var ping = jObject["ping"]!.Value<int>()!;
#if !STEREO
        var color = jObject["color"]!.Value<uint>()!;
        var nickname = jObject["nickname"]!.Value<string>()!;
#endif

        var isSpectator = context is GameSerializationContext.Spectator;
        var isOwner = !isSpectator && context.IsPlayerWithId(id);

        if (isSpectator || isOwner)
        {
            var remainingTicksToRegen = jObject["ticksToRegen"]!.Value<int?>();
#if !STEREO
            var score = jObject["score"]!.Value<int>()!;
#endif

            return new Player(id)
            {
                Ping = ping,
#if CLIENT
                RemainingRespawnTankTicks = remainingTicksToRegen,
#endif
#if !STEREO
                Color = color,
                Nickname = nickname,
                Score = score,
#endif
#if !STEREO
                /* Backwards compatibility */
                VisibilityGrid = jObject["visibility"]?.ToObject<VisibilityPayload>(serializer)?.Grid,
                IsUsingRadar = jObject["isUsingRadar"]?.Value<bool?>(),
#endif
            };
        }

        return new Player(id)
        {
            Ping = ping,
#if !STEREO
            Color = color,
            Nickname = nickname,
#endif
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Player? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["id"] = value!.Id,
            ["ping"] = value.Ping,
#if !STEREO
            ["color"] = value.Color,
            ["nickname"] = value.Nickname,
#endif
        };

        var isSpectator = context is GameSerializationContext.Spectator;
        var isOwner = !isSpectator && context.IsPlayerWithId(value.Id);

#if STEREO
        var isTeammate = !isOwner && context.IsTeammate(value.Id);
        if (isSpectator || isOwner || isTeammate)
#else
        if (isSpectator || isOwner)
#endif
        {
            jObject["ticksToRegen"] = value.Tank.RemainingRegenerationTicks;

#if !STEREO
            jObject["score"] = value.Score;
#endif

#if !STEREO
            /* Backwards compatibility */
            jObject["isUsingRadar"] = value.Tank.GetAbility<RadarAbility>()?.IsActive;
#endif
        }

#if !STEREO
        /* Backwards compatibility */
        if (isSpectator && value.VisibilityGrid is not null)
        {
            var visibilityPayload = new VisibilityPayload(value.VisibilityGrid);
            jObject["visibility"] = JToken.FromObject(visibilityPayload, serializer);
        }
#endif

        jObject.WriteTo(writer);
    }
}
