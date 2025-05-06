namespace GameLogic.Networking;

#if DEBUG

/// <summary>
/// Represents a global ability use payload.
/// </summary>
/// <param name="abilityType">The ability type.</param>
public class GlobalAbilityUsePayload(AbilityType abilityType) : AbilityUsePayload(abilityType)
{
    /// <inheritdoc/>
    public override PacketType Type => PacketType.GlobalAbilityUse;
}

#endif
