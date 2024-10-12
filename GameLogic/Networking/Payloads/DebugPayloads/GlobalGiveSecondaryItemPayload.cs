namespace GameLogic.Networking;

#if DEBUG

/// <summary>
/// Represents a payload to give a secondary item to a player.
/// </summary>
/// <param name="Item">The secondary item to give.</param>
public record class GlobalGiveSecondaryItemPayload(SecondaryItemType Item) : GiveSecondaryItemPayload(Item), IPacketPayload
{
    /// <inheritdoc/>
    public new PacketType Type => PacketType.GlobalGiveSecondaryItem;
}

#endif
