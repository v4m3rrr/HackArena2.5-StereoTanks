using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a packet payload.
/// </summary>
public interface IPacketPayload
{
    /// <summary>
    /// Gets the packet type.
    /// </summary>
    [JsonIgnore]
    PacketType Type { get; }
}
