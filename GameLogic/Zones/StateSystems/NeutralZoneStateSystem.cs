using GameLogic.ZoneStates;

namespace GameLogic.ZoneStateSystems;

/// <summary>
/// Handles the logic for a neutral zone.
/// </summary>
internal class NeutralZoneStateSystem : IStateSystem<NeutralZoneState>
{
    /// <inheritdoc/>
    void IStateSystem.Handle(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        this.Handle(context, (NeutralZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.GetNextState(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        return this.GetNextState(context, (NeutralZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.OnPlayerRemoved(ZoneContext context, ZoneState state, Player player)
    {
        return this.OnPlayerRemoved(context, (NeutralZoneState)state, player);
    }

    /// <inheritdoc/>
    public void Handle(ZoneContext context, NeutralZoneState state, List<Tank> tanksInZone)
    {
    }

    /// <inheritdoc/>
    public ZoneState GetNextState(ZoneContext context, NeutralZoneState state, List<Tank> tanksInZone)
    {
        if (tanksInZone.Count == 1)
        {
            var player = tanksInZone[0].Owner;
            return new BeingCapturedZoneState(player, context.GetRemainingTicks(player));
        }

        return tanksInZone.Count >= 2
            ? new BeingContestedZoneState(null)
            : state;
    }

    /// <inheritdoc/>
    public ZoneState OnPlayerRemoved(ZoneContext context, NeutralZoneState state, Player player)
    {
        return state;
    }
}
