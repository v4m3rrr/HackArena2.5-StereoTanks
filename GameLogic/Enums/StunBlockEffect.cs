namespace GameLogic;

/// <summary>
/// Represents the type of stun effect that a tank can have.
/// </summary>
[Flags]
internal enum StunBlockEffect
{
    /// <summary>
    /// No stun effect.
    /// </summary>
    None = 0,

    /// <summary>
    /// Blocks movement.
    /// </summary>
    Movement = 1,

    /// <summary>
    /// Blocks tank rotation.
    /// </summary>
    TankRotation = 2,

    /// <summary>
    /// Blocks turret rotation.
    /// </summary>
    TurretRotation = 4,

    /// <summary>
    /// Block tank and turret rotation.
    /// </summary>
    Rotation = TankRotation | TurretRotation,

    /// <summary>
    /// Blocks the use of abilities.
    /// </summary>
    AbilityUse = 8,

    /// <summary>
    /// Blocks all actions.
    /// </summary>
    All = Movement | Rotation | AbilityUse,
}
