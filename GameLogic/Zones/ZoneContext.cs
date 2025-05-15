using GameLogic.ZoneStateSystems;

namespace GameLogic;

#if STEREO

/// <summary>
/// Represents the runtime context for a specific <see cref="Zone"/> instance.
/// </summary>
internal class ZoneContext(Zone zone, ScoreSystem scoreSystem)
{
    /// <summary>
    /// The initial neutral resistance that must be
    /// reduced before scoring starts.
    /// </summary>
    public const float InitialNeutralControl = 50f;

    private const float PointsPerTick = 0.3f;

    /// <summary>
    /// Gets the associated zone.
    /// </summary>
    public Zone Zone => zone;

    /// <summary>
    /// Updates the zone state for the current tick.
    /// This includes reducing neutral control and awarding points.
    /// </summary>
    public void Update()
    {
        float totalPressure = zone.Shares.ByTeam.Values.Sum();
        zone.Shares.NeutralControl = Math.Max(0f, InitialNeutralControl - totalPressure);

        if (!zone.Shares.IsScoringAvailable)
        {
            return;
        }

        foreach (Team team in zone.Shares.ByTeam.Keys)
        {
            float normalized = zone.Shares.GetNormalized(team);
            float score = normalized * PointsPerTick;
            scoreSystem.AwardScore(team, score);
        }
    }

    /// <summary>
    /// Adds a number of shares to the specified team.
    /// </summary>
    /// <param name="team">The team to modify.</param>
    /// <param name="value">The number of shares to add.</param>
    public void AddShares(Team team, float value)
    {
        zone.Shares.ByTeam[team] = zone.Shares.ByTeam.TryGetValue(team, out float current)
            ? current + value
            : value;
    }

    /// <summary>
    /// Initializes the zone shares at the beginning of the game.
    /// </summary>
    public void Initialize()
    {
        zone.Shares.NeutralControl = InitialNeutralControl;
        zone.Shares.ByTeam.Clear();
    }
}

#else

/// <summary>
/// Represents the runtime context for a specific <see cref="Zone"/> instance.
/// </summary>
/// <remarks>
/// Used to track per-tick capture progress and to delegate
/// state behavior through <see cref="IStateSystem"/>.
/// </remarks>
internal class ZoneContext(Zone zone, Dictionary<Type, IStateSystem> stateSystems)
{
    private readonly Dictionary<Player, int> remainingTicksToCapture = [];

    /// <summary>
    /// Gets the zone associated with this context.
    /// </summary>
    public Zone Zone => zone;

    private IStateSystem StateSystem => stateSystems[zone.State.GetType()];

    /// <summary>
    /// Returns the number of ticks remaining
    /// for the specified player to capture the zone.
    /// </summary>
    /// <param name="player">The player to query.</param>
    /// <returns>
    /// The number of ticks remaining,
    /// or <see cref="ZoneSystem.TicksToCapture"/> if not tracked yet.
    /// </returns>
    public int GetRemainingTicks(Player player)
    {
        return this.remainingTicksToCapture.TryGetValue(player, out var ticks)
            ? ticks
            : ZoneSystem.TicksToCapture;
    }

    /// <summary>
    /// Gets a list of players who are currently
    /// in the process of capturing or retaking the zone.
    /// </summary>
    /// <returns>
    /// A list of <see cref="Player"/> instances tracked for capture progress.
    /// </returns>
    public List<Player> GetCapturingPlayers()
    {
        return [..this.remainingTicksToCapture.Keys];
    }

    /// <summary>
    /// Gets the player with the lowest number of remaining ticks
    /// needed to capture the zone.
    /// </summary>
    /// <returns>
    /// A tuple containing the <see cref="Player"/> and their remaining ticks,
    /// or <see langword="null"/> if no one is progressing.
    /// </returns>
    public (Player Player, int RemainingTicks)? GetClosestToCapturePlayer()
    {
        var min = this.remainingTicksToCapture
            .OrderBy(kvp => kvp.Value)
            .FirstOrDefault();

        return min.Key is null ? null : (min.Key, min.Value);
    }

    /// <summary>
    /// Applies per-tick logic for the current zone state
    /// by invoking the associated <see cref="IStateSystem"/>.
    /// </summary>
    /// <param name="tanksInZone">The tanks currently inside the zone.</param>
    public void HandleTick(List<Tank> tanksInZone)
    {
        this.StateSystem.Handle(this, zone.State, tanksInZone);
    }

    /// <summary>
    /// Determines and applies the next zone state
    /// based on the current context and tanks present.
    /// Updates the capture progress value for the capturing player if applicable.
    /// </summary>
    /// <param name="tanksInZone">The tanks currently inside the zone.</param>
    public void UpdateState(List<Tank> tanksInZone)
    {
        zone.State = this.StateSystem.GetNextState(this, zone.State, tanksInZone);

        if (zone.State is ICaptureState capturable)
        {
            capturable.RemainingTicks = this.GetRemainingTicks(capturable.BeingCapturedBy);
        }
    }

    /// <summary>
    /// Updates the per-player remaining ticks for the current zone state.
    /// </summary>
    /// <param name="tanksInZone">Tanks currently inside the zone.</param>
    /// <param name="tanksOutsideZone">Tanks currently outside the zone.</param>
    /// <remarks>
    /// Increments remaining ticks for players who are outside the zone
    /// and decrements remaining ticks for the player capturing the zone.
    /// </remarks>
    public void UpdateProgress(List<Tank> tanksInZone, List<Tank> tanksOutsideZone)
    {
        foreach (var tank in tanksOutsideZone)
        {
            if (this.remainingTicksToCapture.TryGetValue(tank.Owner, out int value))
            {
                if (++value > ZoneSystem.TicksToCapture)
                {
                    _ = this.remainingTicksToCapture.Remove(tank.Owner);
                }
                else
                {
                    this.remainingTicksToCapture[tank.Owner] = value;
                }
            }
        }

        if (zone.State is ICaptureState capturable)
        {
            bool isPresent = tanksInZone.Any(t => t.Owner.Equals(capturable.BeingCapturedBy));
            if (isPresent)
            {
                if (!this.remainingTicksToCapture.TryGetValue(capturable.BeingCapturedBy, out int value))
                {
                    this.remainingTicksToCapture[capturable.BeingCapturedBy] = ZoneSystem.TicksToCapture;
                }
                else
                {
                    this.remainingTicksToCapture[capturable.BeingCapturedBy]--;
                }
            }
        }
    }

    /// <summary>
    /// Handles the removal of a player from the zone context,
    /// including cleaning up their capture progress and transitioning the zone state if necessary.
    /// </summary>
    /// <param name="player">The player who has been removed from the game.</param>
    public void OnPlayerRemoved(Player player)
    {
        _ = this.remainingTicksToCapture.Remove(player);
        this.Zone.State = this.StateSystem.OnPlayerRemoved(this, this.Zone.State, player);
    }
}

#endif
