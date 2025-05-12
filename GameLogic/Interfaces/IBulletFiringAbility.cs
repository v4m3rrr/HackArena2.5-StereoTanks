namespace GameLogic;

/// <summary>
/// Represents an ability that fires a <see cref="Bullet"/>
/// from a <see cref="Turret"/>.
/// </summary>
internal interface IBulletFiringAbility : IAbility
{
    /// <summary>
    /// Gets the turret that owns this ability.
    /// </summary>
    Turret Turret { get; }
}
