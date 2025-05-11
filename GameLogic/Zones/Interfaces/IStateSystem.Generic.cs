namespace GameLogic.ZoneStateSystems;

/// <summary>
/// Defines a strongly-typed interface for a zone state system
/// operating on a specific state type.
/// </summary>
/// <typeparam name="T">
/// The specific type of <see cref="ZoneState"/> this system handles.
/// </typeparam>
internal interface IStateSystem<in T> : IStateSystem
    where T : ZoneState
{
    /// <inheritdoc cref="IStateSystem.Handle(ZoneContext, ZoneState, List{Tank})"/>
    void Handle(ZoneContext context, T current, List<Tank> tanks);

    /// <inheritdoc cref="IStateSystem.GetNextState(ZoneContext, ZoneState, List{Tank})"/>
    ZoneState GetNextState(ZoneContext context, T current, List<Tank> tanks);

    /// <inheritdoc cref="IStateSystem.OnPlayerRemoved(ZoneContext, ZoneState, Player)"/>
    ZoneState OnPlayerRemoved(ZoneContext context, T state, Player player);
}
