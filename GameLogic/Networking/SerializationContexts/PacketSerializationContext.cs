namespace GameLogic.Networking;

/// <summary>
/// Represents a packet serialization context.
/// </summary>
/// <param name="TypeOfPacketType">
/// The type as which the packet type is or should be serialized.
/// </param>
internal record class PacketSerializationContext(TypeOfPacketType TypeOfPacketType);
