using System.Numerics;

namespace GameLogic;

/// <summary>
/// Handles score distribution based on in-game events (damage, zone control, etc.).
/// </summary>
internal sealed class ScoreSystem
{
#if STEREO
    private readonly Dictionary<Team, float> fractionalScoreBuffer = [];
#else
    private readonly Dictionary<Player, float> fractionalScoreBuffer = [];
#endif

#if STEREO

    /// <summary>
    /// Awards score to a team.
    /// </summary>
    /// <param name="team">The team to award the score to.</param>
    /// <param name="score">The score to award.</param>
    public void AwardScore(Team team, int score)
    {
        ValidateScore(score);
        team.Score += score;
    }

    /// <summary>
    /// Awards a fractional score to the specified team.
    /// </summary>
    /// <param name="team">The team to award the score to.</param>
    /// <param name="score">The score to award. Can be fractional.</param>
    /// <remarks>
    /// The actual score is accumulated internally and only full integer points are applied to the visible score.
    /// Fractional parts are preserved and applied when they reach a full point.
    /// </remarks>
    public void AwardScore(Team team, float score)
    {
        this.AccumulateFractionalScore(team, score);
    }

#endif

    /// <summary>
    /// Awards score to a team.
    /// </summary>
    /// <param name="player">The player to award the score to.</param>
    /// <param name="score">The score to award.</param>
    public void AwardScore(Player player, int score)
    {
#if STEREO
        this.AwardScore(player.Team, score);
#else
        ValidateScore(score);
        player.Score += score;
#endif
    }

    /// <summary>
    /// Awards a fractional score to the specified player.
    /// </summary>
    /// <param name="player">The player to award the score to.</param>
    /// <param name="score">The score to award. Can be fractional.</param>
    /// <remarks>
    /// The actual score is accumulated internally and only full integer points are applied to the visible score.
    /// Fractional parts are preserved and applied when they reach a full point.
    /// </remarks>
    public void AwardScore(Player player, float score)
    {
#if STEREO
        this.AwardScore(player.Team, score);
#else
        this.AccumulateFractionalScore(player, score);
#endif
    }

#if !STEREO

    /// <summary>
    /// Notifies the score system that a player has been removed from the game.
    /// </summary>
    /// <param name="player">The player who has been removed.</param>
    public void OnPlayerRemoved(Player player)
    {
        _ = this.fractionalScoreBuffer.Remove(player);
    }

#endif

    private static void ValidateScore<T>(T score)
        where T : INumber<T>
    {
        if (score < T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(score), score, "Cannot be negative.");
        }
    }

#if STEREO
    private void AccumulateFractionalScore(Team key, float score)
#else
    private void AccumulateFractionalScore(Player key, float score)
#endif
    {
        ValidateScore(score);

        if (!this.fractionalScoreBuffer.TryAdd(key, score))
        {
            this.fractionalScoreBuffer[key] += score;
        }

        int fullPoints = (int)this.fractionalScoreBuffer[key];

        if (fullPoints >= 1)
        {
            this.AwardScore(key, fullPoints);
            this.fractionalScoreBuffer[key] -= fullPoints;
        }
    }
}
