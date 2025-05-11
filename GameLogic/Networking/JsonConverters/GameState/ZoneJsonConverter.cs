using GameLogic.ZoneStates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking.GameState;

/// <summary>
/// Represents a zone json converter.
/// </summary>
internal class ZoneJsonConverter : JsonConverter<Zone>
{
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
        var status = DeserializeZoneState(statusToken, serializer);

        return new Zone(x, y, width, height, index)
        {
            State = status,
        };
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
            ["status"] = SerializeZoneState(value.State, serializer),
        };

        jObject.WriteTo(writer);
    }

    private static ZoneState DeserializeZoneState(JToken token, JsonSerializer serializer)
    {
        var type = token["type"]!.Value<string>();

        return type switch
        {
            "beingCaptured" => token.ToObject<BeingCapturedZoneState>(serializer)!,
            "captured" => token.ToObject<CapturedZoneState>(serializer)!,
            "beingContested" => token.ToObject<BeingContestedZoneState>(serializer)!,
            "beingRetaken" => token.ToObject<BeingRetakenZoneState>(serializer)!,
            "neutral" => token.ToObject<NeutralZoneState>(serializer)!,
            _ => throw new JsonSerializationException($"Unknown ZoneState type: {type}"),
        };
    }

    private static JObject SerializeZoneState(ZoneState state, JsonSerializer serializer)
    {
        var jObject = JObject.FromObject(state, serializer);
        var type = state.GetType().Name;
        jObject["type"] = char.ToLowerInvariant(type[0]) + type.Split("ZoneState")[0][1..];
        return jObject;
    }
}
