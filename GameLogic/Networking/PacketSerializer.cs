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
    private static readonly IContractResolver ContractResolver
        = new CamelCasePropertyNamesContractResolver();

    /// <summary>
    /// Occurs when an exception is thrown
    /// during serialization or deserialization.
    /// </summary>
    public static event Action<Exception>? ExceptionThrew;

    /// <summary>
    /// Gets the serializer with the specified converters.
    /// </summary>
    /// <param name="converters">The converters to use during serialization.</param>
    /// <returns>The serializer with the specified converters.</returns>
    public static JsonSerializer GetSerializer(IEnumerable<JsonConverter> converters)
    {
        var serializer = new JsonSerializer()
        {
            ContractResolver = ContractResolver,
        };

        foreach (var converter in converters)
        {
            serializer.Converters.Add(converter);
        }

        return serializer;
    }

    /// <summary>
    /// Serializes the specified payload with the specified converters.
    /// </summary>
    /// <param name="payload">The payload to serialize.</param>
    /// <param name="converters">The converters to use during serialization.</param>
    /// <param name="options">The serialization options.</param>
    /// <returns>The serialized packet.</returns>
    public static string Serialize(
        IPacketPayload payload,
        IEnumerable<JsonConverter>? converters = null,
        SerializationOptions? options = null)
    {
        return Serialize(payload, out _, converters, options);
    }

    /// <summary>
    /// Serializes the specified payload with the specified converters.
    /// </summary>
    /// <param name="payload">The payload to serialize.</param>
    /// <param name="serializedPayload">The serialized payload.</param>
    /// <param name="converters">The converters to use during serialization.</param>
    /// <param name="options">The serialization options.</param>
    /// <returns>The serialized packet.</returns>
    public static string Serialize(
        IPacketPayload payload,
        out JObject serializedPayload,
        IEnumerable<JsonConverter>? converters = null,
        SerializationOptions? options = null)
    {
        options ??= SerializationOptions.Default;

        try
        {
            var serializer = GetSerializer(converters ?? []);
            serializedPayload = JObject.FromObject(payload, serializer);

            var packet = new Packet()
            {
                Type = payload.Type,
                Payload = serializedPayload,
            };

            var context = new SerializationContext(options.EnumSerialization);

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = ContractResolver,
                Formatting = options.Formatting,
                Converters = [new PacketJsonConverter(context)],
            };

            return JsonConvert.SerializeObject(packet, settings);
        }
        catch (Exception ex)
        {
            ExceptionThrew?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Converts the specified payload to a byte array.
    /// </summary>
    /// <param name="payload">The payload to convert.</param>
    /// <param name="converters">The converters to use during serialization.</param>
    /// <param name="options">The serialization options.</param>
    /// <returns>The byte array representation of the serialized payload.</returns>
    public static byte[] ToByteArray(
        IPacketPayload payload,
        IEnumerable<JsonConverter> converters,
        SerializationOptions options)
    {
        return ToByteArray(Serialize(payload, converters, options));
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
    /// <param name="serializedPacket">The serialized packet.</param>
    /// <returns>The deserialized packet.</returns>
    public static Packet Deserialize(string serializedPacket)
    {
        try
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = ContractResolver,
            };

            return JsonConvert.DeserializeObject<Packet>(serializedPacket, settings)!;
        }
        catch (Exception ex)
        {
            ExceptionThrew?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Converts the byte array to a packet.
    /// </summary>
    /// <param name="buffer">The byte array to convert.</param>
    /// <returns>The deserialized packet.</returns>
    public static Packet Deserialize(byte[] buffer)
    {
        return Deserialize(FromByteArray(buffer));
    }
}
