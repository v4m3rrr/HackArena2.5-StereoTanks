using GameLogic.ZoneStates;

namespace GameLogic.ZoneStateSystems;

#pragma warning disable CS9113

/// <summary>
/// Handles the logic for a zone that has already been captured.
/// </summary>
internal class CapturedZoneStateSystem(ScoreSystem scoreSystem, HealSystem healSystem)
    : IStateSystem<CapturedZoneState>
{
    /// <inheritdoc/>
    void IStateSystem.Handle(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        this.Handle(context, (CapturedZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.GetNextState(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        return this.GetNextState(context, (CapturedZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.OnPlayerRemoved(ZoneContext context, ZoneState state, Player player)
    {
        return this.OnPlayerRemoved(context, (CapturedZoneState)state, player);
    }

    /// <inheritdoc/>
    public void Handle(ZoneContext context, CapturedZoneState state, List<Tank> tanks)
    {
        scoreSystem.AwardScore(state.Player, 0.5f);

#if !STEREO
        if (state.Player.Tank.Health < 80)
        {
            healSystem.Heal(state.Player.Tank, 0.25f);
        }
#endif
    }

    /// <inheritdoc/>
    public ZoneState GetNextState(ZoneContext context, CapturedZoneState state, List<Tank> tanks)
    {
        if (tanks.Count == 0)
        {
            return state;
        }

        if (tanks.Count == 1)
        {
            var tank = tanks[0];
            var ownerPresent = tank.Owner.Equals(state.Player);
            if (!ownerPresent)
            {
                var remainingTicks = context.GetRemainingTicks(tanks[0].Owner);
                return new BeingRetakenZoneState(state.Player, tank.Owner, remainingTicks);
            }

            return state;
        }

        return new BeingContestedZoneState(state.Player);
    }

    /// <inheritdoc/>
    public ZoneState OnPlayerRemoved(ZoneContext context, CapturedZoneState state, Player player)
    {
        return state.Player.Equals(player)
            ? new NeutralZoneState()
            : state;
    }
}
