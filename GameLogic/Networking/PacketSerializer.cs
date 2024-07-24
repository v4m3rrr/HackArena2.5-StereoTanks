using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GameLogic.Networking;

/// <summary>
/// Represents a packet serializer.
/// </summary>
public static class PacketSerializer
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    private static readonly JsonSerializer Serializer = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    /// <summary>
    /// Serializes the specified payload.
    /// </summary>
    /// <param name="payload">The payload to serialize.</param>
    /// <returns>The serialized payload.</returns>
    public static string Serialize(IPacketPayload payload)
    {
        var packet = new Packet()
        {
            Type = payload.Type,
            Payload = JObject.FromObject(payload, Serializer),
        };

        return JsonConvert.SerializeObject(packet, Settings);
    }

    /// <summary>
    /// Converts the specified payload to a byte array.
    /// </summary>
    /// <param name="payload">The payload to convert.</param>
    /// <returns>The byte array representation of the serialized payload.</returns>
    public static byte[] ToByteArray(IPacketPayload payload)
    {
        return ToByteArray(Serialize(payload));
    }

    /// <summary>
    /// Converts the serialized packet to a byte array.
    /// </summary>
    /// <param name="serializedPacket">The serialized packet.</param>
    /// <returns>The byte array representation of the serialized packet.</returns>
    public static byte[] ToByteArray(string serializedPacket)
    {
        return Encoding.UTF8.GetBytes(serializedPacket);
    }

    /// <summary>
    /// Converts the byte array to a serialized packet.
    /// </summary>
    /// <param name="buffer">
    /// The byte array to convert.
    /// </param>
    /// <returns>The serialized packet.</returns>
    public static string FromByteArray(byte[] buffer)
    {
        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// Deserializes the serialized packet.
    /// </summary>
    /// <param name="serialiedPacket">The serialized packet.</param>
    /// <returns>The deserialized packet.</returns>
    public static Packet Deserialize(string serialiedPacket)
    {
        return JsonConvert.DeserializeObject<Packet>(serialiedPacket, Settings)!;
    }

    /// <summary>
    /// Converts the byte array to a packet.
    /// </summary>
    /// <param name="buffer">The byte array to convert</param>
    /// <returns>The deserialized packet.</returns>
    public static Packet Deserialize(byte[] buffer)
    {
        return Deserialize(FromByteArray(buffer));
    }
}
