using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a packet.
/// </summary>
public class Packet
{
    /// <summary>
    /// Occurs when the payload could not
    /// be converted to the specified type.
    /// </summary>
    public static event Action<Exception>? GetPayloadFailed;

    /// <summary>
    /// Gets the packet type.
    /// </summary>
    public PacketType Type { get; init; }

    /// <summary>
    /// Gets the packet payload.
    /// </summary>
    public JObject Payload { get; init; } = [];

    /// <summary>
    /// Gets the payload as the specified type.
    /// </summary>
    /// <typeparam name="T">The specified payload type.</typeparam>
    /// <returns>The payload as the specified type.</returns>
    public T GetPayload<T>()
        where T : IPacketPayload
    {
        try
        {
            return this.Payload.ToObject<T>()!;
        }
        catch (Exception ex)
        {
            GetPayloadFailed?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets the payload as the specified type.
    /// </summary>
    /// <typeparam name="T">The specified payload type.</typeparam>
    /// <param name="serializer">The serializer to use.</param>
    /// <returns>The payload as the specified type.</returns>
    public T GetPayload<T>(JsonSerializer serializer)
        where T : IPacketPayload
    {
        try
        {
            return this.Payload.ToObject<T>(serializer)!;
        }
        catch (Exception ex)
        {
            GetPayloadFailed?.Invoke(ex);
            throw;
        }
    }
}
