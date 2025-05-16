using GameLogic.ZoneStates;
using GameLogic.ZoneStateSystems;

namespace GameLogic;

#pragma warning disable CS9113

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

#if !STEREO

    private readonly Dictionary<Type, IStateSystem> stateSystems = new()
    {
        { typeof(NeutralZoneState), new NeutralZoneStateSystem() },
        { typeof(BeingCapturedZoneState), new BeingCapturedZoneStateSystem() },
        { typeof(CapturedZoneState), new CapturedZoneStateSystem(scoreSystem, healSystem) },
        { typeof(BeingRetakenZoneState), new BeingRetakenZoneStateSystem() },
        { typeof(BeingContestedZoneState), new BeingContestedZoneStateSystem() },
    };

#endif

    /// <summary>
    /// Updates all zones in the grid.
    /// </summary>
    public void Update()
    {
        foreach (var zone in grid.Zones)
        {
            if (!this.contexts.Any(z => z.Zone.Equals(zone)))
            {
#if STEREO
                var context = new ZoneContext(zone, scoreSystem);
                context.Initialize();
                this.contexts.Add(context);
#else
                this.contexts.Add(new ZoneContext(zone, this.stateSystems));
#endif
            }
        }

        _ = this.contexts.RemoveAll(z => !grid.Zones.Contains(z.Zone));

        foreach (var ctx in this.contexts)
        {
#if STEREO
            ctx.Update();
#else
            var tanksInZone = grid.Tanks.Where(ctx.Zone.Contains).ToList();
            var tanksOutsideZone = grid.Tanks.Where(t => !ctx.Zone.Contains(t)).ToList();
            ctx.HandleTick(tanksInZone);
            ctx.UpdateProgress(tanksInZone, tanksOutsideZone);
            ctx.UpdateState(tanksInZone);
#endif
        }
    }

#if STEREO

    /// <summary>
    /// Attempts to capture a zone with the specified tank.
    /// </summary>
    /// <param name="zone">The zone to capture.</param>
    /// <param name="tank">The tank attempting to capture the zone.</param>
    public void TryCaptureZone(Zone zone, Tank tank)
    {
        var ctx = this.contexts.FirstOrDefault(z => z.Zone.Equals(zone));

        if (ctx is null)
        {
            return;
        }

        var baseInfluence = tank.Type switch
        {
            TankType.Light => 0.0082f,
            TankType.Heavy => 0.0131f,
            _ => 1.0f,
        };

        var team = tank.Owner.Team;
        float value;
        if (zone.Shares.IsScoringAvailable)
        {
            var currentShareRatio = ctx.Zone.Shares.GetNormalized(team);
            var multiplier = 1.25f - (currentShareRatio / 2f);
            value = baseInfluence * multiplier * zone.Shares.TotalShares;
        }
        else
        {
            value = baseInfluence * ZoneContext.InitialNeutralControl;
        }

        ctx.AddShares(team, value);
    }

#endif

#if STEREO

    /// <summary>
    /// Notifies all zone contexts that the specified team has been removed from the game.
    /// Each context will clear any capture-related state and update its zone state accordingly.
    /// </summary>
    /// <param name="team">The team that has been removed.</param>
    public void OnTeamRemoved(Team team)
    {
        foreach (var ctx in this.contexts)
        {
            ctx.OnTeamRemoved(team);
        }
    }

#else

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

#endif
}
