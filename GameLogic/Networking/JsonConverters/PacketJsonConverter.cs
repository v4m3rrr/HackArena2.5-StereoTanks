using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonNamingPolicy = System.Text.Json.JsonNamingPolicy;

namespace GameLogic.Networking;

/// <summary>
/// Represents a packet json converter.
/// </summary>
/// <param name="context">The serialization context.</param>
internal class PacketJsonConverter(PacketSerializationContext context) : JsonConverter<Packet>
{
    /// <inheritdoc/>
    public override Packet? ReadJson(JsonReader reader, Type objectType, Packet? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        var rawType = jObject["type"]!.Value<string>()!;
        PacketType type = context.TypeOfPacketType switch
        {
            TypeOfPacketType.Int => (PacketType)int.Parse(rawType),
            TypeOfPacketType.String => Enum.Parse<PacketType>(rawType, ignoreCase: true),
            _ => throw new InvalidOperationException("Invalid packet type."),
        };

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
            ["type"] = context.TypeOfPacketType switch
            {
                TypeOfPacketType.Int => (int)value!.Type,
                TypeOfPacketType.String => JsonNamingPolicy.CamelCase.ConvertName(value!.Type.ToString()),
                _ => throw new InvalidOperationException("Invalid packet type."),
            },
        };

        if (value!.Type.HasPayload())
        {
            jObject["payload"] = JObject.FromObject(value!.Payload, serializer);
        }

        jObject.WriteTo(writer);
    }
}
