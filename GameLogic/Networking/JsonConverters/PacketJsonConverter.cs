using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a packet json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class PacketJsonConverter(SerializationContext context) : JsonConverter<Packet>
{
    /// <inheritdoc/>
    public override Packet? ReadJson(JsonReader reader, Type objectType, Packet? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var rawType = jObject["type"]!.Value<string>()!;
        var type = JsonConverterUtils.ReadEnum<PacketType>(rawType);

        if (!type.HasPayload())
        {
            return new Packet() { Type = type };
        }

        var payload = jObject["payload"]!.ToObject<IPacketPayload>(serializer)!;

        return new Packet()
        {
            Type = type,
            Payload = JObject.FromObject(payload, serializer),
        };
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Packet? value, JsonSerializer serializer)
    {
        var jObject = new JObject
        {
            ["type"] = JsonConverterUtils.WriteEnum(value!.Type, context.EnumSerialization),
        };

        if (value!.Type.HasPayload())
        {
            jObject["payload"] = JObject.FromObject(value!.Payload, serializer);
        }

        jObject.WriteTo(writer);
    }
}
