using GameLogic.ZoneStates;

namespace GameLogic.ZoneStateSystems;

/// <summary>
/// Handles the logic for a zone currently being captured by a player.
/// </summary>
internal class BeingCapturedZoneStateSystem : IStateSystem<BeingCapturedZoneState>
{
    /// <inheritdoc/>
    void IStateSystem.Handle(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        this.Handle(context, (BeingCapturedZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.GetNextState(ZoneContext context, ZoneState state, List<Tank> tanks)
    {
        return this.GetNextState(context, (BeingCapturedZoneState)state, tanks);
    }

    /// <inheritdoc/>
    ZoneState IStateSystem.OnPlayerRemoved(ZoneContext context, ZoneState state, Player player)
    {
        return this.OnPlayerRemoved(context, (BeingCapturedZoneState)state, player);
    }

    /// <inheritdoc/>
    public void Handle(ZoneContext context, BeingCapturedZoneState state, List<Tank> tanks)
    {
    }

    /// <inheritdoc/>
    public ZoneState GetNextState(ZoneContext context, BeingCapturedZoneState state, List<Tank> tanks)
    {
        if (tanks.Count == 0)
        {
            var closestToCapture = context.GetClosestToCapturePlayer();
            return closestToCapture is { } c
                ? new BeingCapturedZoneState(c.Player, c.RemainingTicks)
                : new NeutralZoneState();
        }

        if (tanks.Count == 1)
        {
            var tank = tanks[0];
            var ownerPresent = tanks[0].Owner.Equals(state.Player);

            if (ownerPresent)
            {
                var remainingTicks = context.GetRemainingTicks(state.Player);
                return remainingTicks == 0
                    ? new CapturedZoneState(state.Player)
                    : state;
            }
            else
            {
                var remainingTicks = context.GetRemainingTicks(tank.Owner);
                return new BeingCapturedZoneState(tank.Owner, remainingTicks);
            }
        }

        return new BeingContestedZoneState(null);
    }

    /// <inheritdoc/>
    public ZoneState OnPlayerRemoved(ZoneContext context, BeingCapturedZoneState state, Player player)
    {
        if (!state.Player.Equals(player))
        {
            return state;
        }

        var closestToCapture = context.GetClosestToCapturePlayer();
        return closestToCapture is { } c
            ? new BeingCapturedZoneState(c.Player, c.RemainingTicks)
            : new NeutralZoneState();
    }
}
