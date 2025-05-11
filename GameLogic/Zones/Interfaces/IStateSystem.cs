namespace GameLogic.ZoneStateSystems;

/// <summary>
/// Defines a contract for a zone state system.
/// </summary>
internal interface IStateSystem
{
    /// <summary>
    /// Handles per-tick logic for the current <see cref="ZoneState"/>.
    /// </summary>
    /// <param name="context">The context associated with the zone being updated.</param>
    /// <param name="state">The current state of the zone.</param>
    /// <param name="tanks">The list of <see langword="Tank"/> instances currently inside the zone.</param>
    void Handle(ZoneContext context, ZoneState state, List<Tank> tanks);

    /// <summary>
    /// Determines the next state of the zone based on its current state and the provided context.
    /// </summary>
    /// <param name="context">The context associated with the zone being updated.</param>
    /// <param name="state">The current state of the zone.</param>
    /// <param name="tanks">The list of <see langword="Tank"/> instances currently inside the zone.</param>
    /// <returns>The next <see cref="ZoneState"/> to transition into.</returns>
    ZoneState GetNextState(ZoneContext context, ZoneState state, List<Tank> tanks);

    /// <summary>
    /// Handles the transition logic when a given player is removed from the game.
    /// </summary>
    /// <param name="context">The zone context associated with the current zone.</param>
    /// <param name="state">The current state of the zone.</param>
    /// <param name="player">The player who was removed.</param>
    /// <returns>The new state after the removal, or the original state if unaffected.</returns>
    ZoneState OnPlayerRemoved(ZoneContext context, ZoneState state, Player player);
}
