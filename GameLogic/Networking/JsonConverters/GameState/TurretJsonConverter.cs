using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

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

        var direction = (Direction)jsonObject["direction"]!.Value<int>()!;

        var bulletCount = jsonObject["bulletCount"]?.Value<int>();
        var remainingTicksToRegenBullet = jsonObject["ticksToRegenBullet"]?.Value<int?>();

        if (bulletCount is null)
        {
            // Player perspective for other players
            return new Turret(direction);
        }

        return new Turret(direction, bulletCount.Value, remainingTicksToRegenBullet);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Turret? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["direction"] = (int)value!.Direction,
        };

        if (context is GameSerializationContext.Spectator || context.IsPlayerWithId(value.Tank.Owner.Id))
        {
            jObject["bulletCount"] = value.BulletCount;
            jObject["ticksToRegenBullet"] = value.RemainingTicksToRegenBullet;
        }

        jObject.WriteTo(writer);
    }
}
