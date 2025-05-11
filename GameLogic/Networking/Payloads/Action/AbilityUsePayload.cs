using GameLogic.Networking.Exceptions;

namespace GameLogic.Networking;

/// <summary>
/// Represents an ability use payload.
/// </summary>
/// <param name="abilityType">The type of the ability.</param>
public class AbilityUsePayload(AbilityType abilityType) : ActionPayload
{
    /// <inheritdoc/>
    public override PacketType Type => PacketType.AbilityUse;

    /// <summary>
    /// Gets the ability type.
    /// </summary>
    public AbilityType AbilityType { get; } = abilityType;

    /// <inheritdoc/>
    internal override void ValidateEnums()
    {
        if (!Enum.IsDefined(this.AbilityType))
        {
            throw new PayloadEnumValidationError<AbilityType>(this.AbilityType);
        }
    }
}
