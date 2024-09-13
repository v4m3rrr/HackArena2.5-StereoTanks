using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a zone json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class ZoneJsonConverter(SerializationContext context) : JsonConverter<Zone>
{
    private readonly SerializationContext context = context;

    /// <inheritdoc/>
    public override Zone? ReadJson(JsonReader reader, Type objectType, Zone? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var x = jObject["x"]!.Value<int>()!;
        var y = jObject["y"]!.Value<int>()!;
        var width = jObject["width"]!.Value<int>()!;
        var height = jObject["height"]!.Value<int>()!;
        var index = jObject["index"]!.Value<char>()!;

        var statusToken = jObject["status"]!;
        var status = DeserializeZoneStatus(statusToken, serializer);

        return new Zone(x, y, width, height, index, status);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Zone? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["x"] = value!.X,
            ["y"] = value!.Y,
            ["width"] = value!.Width,
            ["height"] = value!.Height,
            ["index"] = value!.Index,
            ["status"] = SerializeZoneStatus(value.Status, serializer),
        };

        jObject.WriteTo(writer);
    }

    private static ZoneStatus DeserializeZoneStatus(JToken token, JsonSerializer serializer)
    {
        var type = token["type"]!.Value<string>();

        return type switch
        {
            "beingCaptured" => token.ToObject<ZoneStatus.BeingCaptured>(serializer)!,
            "captured" => token.ToObject<ZoneStatus.Captured>(serializer)!,
            "beingContested" => new ZoneStatus.BeingContested(
                token["capturedBy"]?.Type == JTokenType.Null ? null : token["capturedBy"]?.ToObject<Player>(serializer)),
            "beingRetaken" => token.ToObject<ZoneStatus.BeingRetaken>(serializer)!,
            "neutral" => token.ToObject<ZoneStatus.Neutral>(serializer)!,
            _ => throw new JsonSerializationException($"Unknown ZoneStatus type: {type}"),
        };
    }

    private static JObject SerializeZoneStatus(ZoneStatus status, JsonSerializer serializer)
    {
        var jObject = JObject.FromObject(status, serializer);
        var type = status.GetType().Name;
        jObject["type"] = char.ToLowerInvariant(type[0]) + type.Substring(1);
        return jObject;
    }
}
