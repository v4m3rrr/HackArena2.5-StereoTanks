namespace GameLogic;

#if STEREO

/// <summary>
/// Represents share distribution in a zone.
/// </summary>
/// <remarks>
/// Used to determine which team receives points based on their control percentage.
/// </remarks>
public class ZoneShares
{
#if CLIENT

    /// <summary>
    /// Gets or sets the normalized neutral share.
    /// </summary>
    /// <value>The normalized neutral share (0–1).</value>
    public float NormalizedNeutral { get; set; }

    /// <summary>
    /// Gets or sets the normalized shares by team.
    /// </summary>
    public Dictionary<Team, float> NormalizedByTeam { get; set; } = [];

    /// <summary>
    /// Gets the normalized shares by team name.
    /// </summary>
    /// <remarks>
    /// Used primarily for serialization purposes.
    /// </remarks>
    public Dictionary<string, float> NormalizedByTeamName { get; init; } = [];

#endif

    /// <summary>
    /// Gets or sets the neutral resistance of the zone.
    /// </summary>
    /// <remarks>
    /// This is treated as initial "control" that must be overcome by teams.
    /// </remarks>
    public float NeutralControl { get; set; } = 0;

    /// <summary>
    /// Gets or sets the team shares for the zone.
    /// </summary>
    public Dictionary<Team, float> ByTeam { get; set; } = [];

    /// <summary>
    /// Gets the percentage share of neutral control in the zone.
    /// </summary>
    public float NeutralShare => this.NeutralControl / this.TotalShares;

    /// <summary>
    /// Gets the total amount of all shares, including neutral control.
    /// </summary>
    public float TotalShares => this.NeutralControl + this.ByTeam.Values.Sum();

    /// <summary>
    /// Gets a value indicating whether any team is eligible to receive points.
    /// </summary>
    /// <remarks>
    /// This is true only if the neutral control has been fully removed.
    /// </remarks>
    public bool IsScoringAvailable => this.NeutralControl <= 0;

    /// <summary>
    /// Gets the normalized share (0–1) of a specific team.
    /// Returns 0 if the total share is zero.
    /// </summary>
    /// <param name="team">The team to check.</param>
    /// <returns>The normalized share of the team.</returns>
    public float GetNormalized(Team team)
    {
        var sum = this.TotalShares;

        if (sum == 0)
        {
            return 0;
        }

        return this.ByTeam.TryGetValue(team, out var value) ? value / sum : 0;
    }

#if CLIENT

    /// <summary>
    /// Updates this instance from a snapshotd.
    /// </summary>
    /// <param name="snapshot">The snapshot to update from.</param>
    public void UpdateFrom(ZoneShares snapshot)
    {
        this.NormalizedNeutral = snapshot.NormalizedNeutral;
        this.NormalizedByTeam = snapshot.NormalizedByTeam.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value);
    }

#endif
}

#endif
