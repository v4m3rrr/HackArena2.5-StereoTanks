using System.Diagnostics;

namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a declared tank stub.
/// </summary>
[DebuggerDisplay("{Type}")]
public class DeclaredTankStub : Tank
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeclaredTankStub"/> class.
    /// </summary>
    /// <param name="owner">The owner of the tank.</param>
    /// <param name="declaredTankType">The declared tank type.</param>
    public DeclaredTankStub(Player owner, TankType declaredTankType)
        : base(-1, -1, owner.Id, Direction.Up)
    {
        this.Type = declaredTankType;
        this.Owner = owner;
    }

    /// <inheritdoc/>
    /// <value>The declared tank type.</value>
    public override TankType Type { get; }

#if SERVER && DEBUG

    /// <inheritdoc/>
    public override void ChargeAbility(AbilityType abilityType)
    {
    }

#endif

    /// <inheritdoc/>
    internal override void UpdateAbilitiesCooldown()
    {
    }
}

#endif
