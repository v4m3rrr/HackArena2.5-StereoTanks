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
        where T : class, IPacketPayload
    {
        return this.Get<T>(null, out _);
    }

    /// <summary>
    /// Gets the payload as the specified type.
    /// </summary>
    /// <typeparam name="T">The specified payload type.</typeparam>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>The payload as the specified type.</returns>
    public T GetPayload<T>(out Exception? exception)
        where T : class, IPacketPayload
    {
        return this.Get<T>(null, out exception);
    }

    /// <summary>
    /// Gets the payload as the specified type.
    /// </summary>
    /// <typeparam name="T">The specified payload type.</typeparam>
    /// <param name="serializer">The serializer to use.</param>
    /// <returns>The payload as the specified type.</returns>
    public T GetPayload<T>(JsonSerializer serializer)
        where T : class, IPacketPayload
    {
        return this.GetPayload<T>(serializer, out _);
    }

    /// <summary>
    /// Gets the payload as the specified type.
    /// </summary>
    /// <typeparam name="T">The specified payload type.</typeparam>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>The payload as the specified type.</returns>
    public T GetPayload<T>(JsonSerializer serializer, out Exception? exception)
        where T : class, IPacketPayload
    {
        return this.Get<T>(serializer, out exception);
    }

    private T Get<T>(JsonSerializer? serializer, out Exception? exception)
        where T : class, IPacketPayload
    {
        try
        {
            var payload = this.GetPayloadObject<T>(serializer);
            (payload as IActionPayload)?.ValidateEnums();
            exception = null;
            return payload;
        }
        catch (Exception ex)
        {
            exception = ex;
            GetPayloadFailed?.Invoke(ex);
            return null!;
        }
    }

    private T GetPayloadObject<T>(JsonSerializer? serializer = null)
        where T : class, IPacketPayload
    {
        return serializer is null
            ? this.Payload.ToObject<T>()!
            : this.Payload.ToObject<T>(serializer)!;
    }
}
