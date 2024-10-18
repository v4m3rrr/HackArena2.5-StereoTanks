namespace GameLogic.Networking;

#if DEBUG

/// <summary>
/// Represents a global ability use payload.
/// </summary>
/// <param name="AbilityType">The ability type.</param>
public record class GlobalAbilityUsePayload(AbilityType AbilityType) : AbilityUsePayload(AbilityType), IPacketPayload
{
    /// <inheritdoc/>
    public new PacketType Type { get; private set; } = PacketType.GlobalAbilityUse;
}

#endif
