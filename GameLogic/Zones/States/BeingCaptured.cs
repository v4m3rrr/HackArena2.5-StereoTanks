using Newtonsoft.Json;

namespace GameLogic.ZoneStates;
/// <summary>
/// Represents a zone that is being captured.
/// </summary>
/// <param name="player">The player that is capturing the zone.</param>
/// <param name="remainingTicks">The number of ticks remaining until the zone is captured.</param>
public class BeingCaptured(Player player, int remainingTicks) : ZoneState
{
    /// <summary>
    /// Gets the player that is being capturing the zone.
    /// </summary>
    [JsonIgnore]
    public Player Player { get; internal set; } = player;

    /// <summary>
    /// Gets the number of ticks remaining until the zone is captured.
    /// </summary>
    public int RemainingTicks { get; internal set; } = remainingTicks;

    /// <summary>
    /// Gets the ID of the player that is being capturing the zone.
    /// </summary>
    [JsonProperty]
    public string PlayerId { get; private set; } = player?.Id!;

    /// <inheritdoc/>
    public override void Handle(ZoneSystem.Context context, List<Tank> tanksInZone)
    {
        if (tanksInZone.Count == 1 && tanksInZone[0].Owner == player)
        {
            this.RemainingTicks = context.DecrementProgress(player);
        }
    }

    /// <inheritdoc/>
    public override ZoneState GetNextState(ZoneSystem.Context context, List<Tank> tanksInZone)
    {
        if (remainingTicks <= 0)
        {
            context.ClearProgress();
            return new Captured(player);
        }

        if (tanksInZone.Count >= 2)
            return new BeingContested(null);

        if (tanksInZone.Count == 0)
        {
            if ((this.RemainingTicks = context.IncreaseProgress(player)) > ZoneSystem.TicksToCapture)
            {
                context.RemoveProgress(player);
                return this;
            }

            return new Neutral();
        }

        return this;
    }
}