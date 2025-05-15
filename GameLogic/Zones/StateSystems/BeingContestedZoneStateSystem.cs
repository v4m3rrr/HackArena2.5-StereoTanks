using GameLogic.ZoneStates;

namespace GameLogic.ZoneStateSystems;

#if !STEREO

/// <summary>
/// Handles the logic for a contested zone where multiple tanks are present.
/// </summary>
internal class BeingContestedZoneStateSystem : IStateSystem<BeingContestedZoneState>
{
    /// <inheritdoc/>
    void IStateSystem.Handle(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        this.Handle(context, (BeingContestedZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.GetNextState(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        return this.GetNextState(context, (BeingContestedZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.OnPlayerRemoved(ZoneContext context, ZoneState state, Player player)
    {
        return this.OnPlayerRemoved(context, (BeingContestedZoneState)state, player);
    }

    /// <inheritdoc/>
    public void Handle(ZoneContext context, BeingContestedZoneState state, List<Tank> tanks)
    {
    }

    /// <inheritdoc/>
    public ZoneState GetNextState(ZoneContext context, BeingContestedZoneState state, List<Tank> tanks)
    {
        if (tanks.Count == 0)
        {
            if (state.CapturedBy is null)
            {
                var closestToCapture = context.GetClosestToCapturePlayer();
                return closestToCapture is { } c
                    ? new BeingCapturedZoneState(c.Player, c.RemainingTicks)
                    : new NeutralZoneState();
            }

            return new CapturedZoneState(state.CapturedBy);
        }

        if (tanks.Count == 1)
        {
            var tank = tanks[0];
            var ownerPresent = tanks[0].Owner.Equals(state.CapturedBy);

            if (ownerPresent)
            {
                if (state.CapturedBy is null)
                {
                    var remainingTicks = context.GetRemainingTicks(tank.Owner);
                    return new BeingCapturedZoneState(tank.Owner, remainingTicks);
                }

                return new CapturedZoneState(state.CapturedBy);
            }
            else
            {
                var remainingTicks = context.GetRemainingTicks(tank.Owner);
                return state.CapturedBy is null
                    ? new BeingCapturedZoneState(tank.Owner, remainingTicks)
                    : new BeingRetakenZoneState(state.CapturedBy, tank.Owner, remainingTicks);
            }
        }

        return state;
    }

    /// <inheritdoc/>
    public ZoneState OnPlayerRemoved(ZoneContext context, BeingContestedZoneState state, Player player)
    {
        return state.CapturedBy is not null && state.CapturedBy.Equals(player)
            ? new BeingContestedZoneState(null)
            : state;
    }
}

#endif
