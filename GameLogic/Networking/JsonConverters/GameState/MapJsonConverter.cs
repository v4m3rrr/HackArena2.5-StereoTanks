using GameLogic.Networking.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

#pragma warning disable CS9113

/// <summary>
/// Represents a map JSON converter.
/// </summary>
/// <param name="context">The game serialization context.</param>
internal class MapJsonConverter(GameSerializationContext context) : JsonConverter<MapPayload>
{
    /// <inheritdoc/>
    public override MapPayload? ReadJson(JsonReader reader, Type objectType, MapPayload? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var tiles = jObject["tiles"]!.ToObject<TilesPayload>(serializer)!;
        var zones = jObject["zones"]!.ToObject<List<Zone>>(serializer)!;

        return new MapPayload(tiles, zones)
        {
#if !STEREO
            /* Backwards compatibility */
            Visibility = context is GameSerializationContext.Player
                ? jObject["visibility"]!.ToObject<VisibilityPayload>(serializer)!
                : null,
#endif
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, MapPayload? value, JsonSerializer serializer)
    {
        var jObject = new JObject()
        {
            ["tiles"] = JToken.FromObject(value!.Tiles, serializer),
            ["zones"] = JToken.FromObject(value!.Zones, serializer),
        };

#if !STEREO
        /* Backwards compatibility */
        if (context is GameSerializationContext.Player)
        {
            jObject["visibility"] = JToken.FromObject(value!.Visibility!, serializer);
        }
#endif

        jObject.WriteTo(writer);
    }
}
