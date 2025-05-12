namespace GameLogic;

/// <summary>
/// Represents an executable capability
/// that can be owned by a turret.
/// </summary>
internal interface IFireableAbility : IAbility
{
    /// <summary>
    /// Gets the turret that owns this ability.
    /// </summary>
    Turret Turret { get; }
}
