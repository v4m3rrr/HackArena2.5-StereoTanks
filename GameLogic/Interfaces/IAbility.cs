namespace GameLogic;

/// <summary>
/// Represents an ability that can be executed by a game entity,
/// such as a <see cref="Tank"/> or its <see cref="Turret"/>.
/// </summary>
internal interface IAbility
{
    /// <summary>
    /// Gets a value indicating whether
    /// the ability can currently be used.
    /// </summary>
    bool CanUse { get; }

    /// <summary>
    /// Executes the ability's action.
    /// </summary>
    /// <remarks>
    /// This method typically modifies the internal state of the ability,
    /// such as starting a cooldown or marking it as used. Consumers should
    /// check <see cref="CanUse"/> before calling this method.
    /// </remarks>
    void Use();

    /// <summary>
    /// Resets the ability.
    /// </summary>
    void Reset()
    {
    }
}
