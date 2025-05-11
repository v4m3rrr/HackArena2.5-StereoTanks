namespace GameLogic.Networking;

#if DEBUG && STEREO

/// <summary>
/// Represents a payload to fully regenerate an ability.
/// </summary>
/// <param name="AbilityType">The type of the ability to regenerate.</param>
public record class FullyRegenerateAbilityPayload(AbilityType AbilityType) : IPacketPayload
{
    /// <summary>
    /// Gets the type of the packet.
    /// </summary>
    public PacketType Type => PacketType.FullyRegenerateAbility;
}

#endif
