using GameLogic.ZoneStates;
using GameLogic.ZoneStateSystems;

namespace GameLogic;

/// <summary>
/// Handles capture logic for zones.
/// </summary>
/// <param name="grid">The grid containing the zones.</param>
/// <param name="scoreSystem">The score system for managing scores.</param>
/// <param name="healSystem">The heal system for managing healing.</param>
internal sealed class ZoneSystem(Grid grid, ScoreSystem scoreSystem, HealSystem healSystem)
{
    /// <summary>
    /// The number of ticks required to capture a zone.
    /// </summary>
    public const int TicksToCapture = 50;

    private readonly List<ZoneContext> contexts = [];
    private readonly Dictionary<Type, IStateSystem> stateSystems = new()
    {
        { typeof(NeutralZoneState), new NeutralZoneStateSystem() },
        { typeof(BeingCapturedZoneState), new BeingCapturedZoneStateSystem() },
        { typeof(CapturedZoneState), new CapturedZoneStateSystem(scoreSystem, healSystem) },
        { typeof(BeingRetakenZoneState), new BeingRetakenZoneStateSystem() },
        { typeof(BeingContestedZoneState), new BeingContestedZoneStateSystem() },
    };

    /// <summary>
    /// Updates all zones in the grid.
    /// </summary>
    public void Update()
    {
        foreach (var zone in grid.Zones)
        {
            if (!this.contexts.Any(z => z.Zone.Equals(zone)))
            {
                this.contexts.Add(new ZoneContext(zone, this.stateSystems));
            }
        }

        _ = this.contexts.RemoveAll(z => !grid.Zones.Contains(z.Zone));

        foreach (var ctx in this.contexts)
        {
            var tanksInZone = grid.Tanks.Where(ctx.Zone.Contains).ToList();
            var tanksOutsideZone = grid.Tanks.Where(t => !ctx.Zone.Contains(t)).ToList();

            ctx.HandleTick(tanksInZone);
            ctx.UpdateProgress(tanksInZone, tanksOutsideZone);
            ctx.UpdateState(tanksInZone);
        }
    }

    /// <summary>
    /// Notifies all zone contexts that the specified player has been removed from the game.
    /// Each context will clear any capture-related state and update its zone state accordingly.
    /// </summary>
    /// <param name="player">The player who has been removed.</param>
    public void OnPlayerRemoved(Player player)
    {
        foreach (var ctx in this.contexts)
        {
            ctx.OnPlayerRemoved(player);
        }
    }
}
