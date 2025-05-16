using GameLogic.ZoneStates;

namespace GameLogic.ZoneStateSystems;

#if !STEREO

/// <summary>
/// Handles the logic for a captured zone that is being retaken by another player.
/// </summary>
internal class BeingRetakenZoneStateSystem : IStateSystem<BeingRetakenZoneState>
{
    /// <inheritdoc/>
    void IStateSystem.Handle(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        this.Handle(context, (BeingRetakenZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.GetNextState(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        return this.GetNextState(context, (BeingRetakenZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.OnPlayerRemoved(ZoneContext context, ZoneState state, Player player)
    {
        return this.OnPlayerRemoved(context, (BeingRetakenZoneState)state, player);
    }

    /// <inheritdoc/>
    public void Handle(ZoneContext context, BeingRetakenZoneState state, List<Tank> tanks)
    {
    }

    /// <inheritdoc/>
    public ZoneState GetNextState(ZoneContext context, BeingRetakenZoneState state, List<Tank> tanks)
    {
        if (tanks.Count == 0)
        {
            var closestToCapture = context.GetClosestToCapturePlayer();
            return closestToCapture is { } c
                ? new BeingRetakenZoneState(state.CapturedBy, c.Player, c.RemainingTicks)
                : new CapturedZoneState(state.CapturedBy);
        }

        if (tanks.Count == 1)
        {
            var tank = tanks[0];
            var ownerPresent = tanks[0].Owner.Equals(state.CapturedBy);

            if (ownerPresent)
            {
                return new CapturedZoneState(state.CapturedBy);
            }
            else
            {
                var remainingTicks = context.GetRemainingTicks(tank.Owner);
                return remainingTicks == 0
                    ? new CapturedZoneState(tank.Owner)
                    : new BeingRetakenZoneState(state.CapturedBy, tank.Owner, remainingTicks);
            }
        }

        return new BeingContestedZoneState(state.CapturedBy);
    }

    /// <inheritdoc/>
    public ZoneState OnPlayerRemoved(ZoneContext context, BeingRetakenZoneState state, Player player)
    {
        if (state.CapturedBy.Equals(player))
        {
            var remainingTicks = context.GetRemainingTicks(player);
            return new BeingCapturedZoneState(state.RetakenBy, remainingTicks);
        }

        return state.RetakenBy.Equals(player)
            ? new CapturedZoneState(state.CapturedBy)
            : state;
    }
}

#endif
