namespace GameLogic.Networking;

#if DEBUG && STEREO

/// <summary>
/// Represents a payload to charge a player's ability.
/// </summary>
/// <param name="AbilityType">The type of the ability to charge.</param>
public record class ChargeAbilityPayload(AbilityType AbilityType) : IPacketPayload
{
    /// <summary>
    /// Gets the type of the packet.
    /// </summary>
    public PacketType Type => PacketType.ChargeAbility;
}

#endif
