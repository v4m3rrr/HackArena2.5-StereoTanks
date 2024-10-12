namespace GameLogic.Networking;

#if DEBUG

/// <summary>
/// Represents a payload to give a secondary item to a player.
/// </summary>
/// <param name="Item">The secondary item to give.</param>
public record class GiveSecondaryItemPayload(SecondaryItemType Item) : IPacketPayload
{
    /// <summary>
    /// Gets the type of the packet.
    /// </summary>
    public PacketType Type => PacketType.GiveSecondaryItem;
}

#endif
