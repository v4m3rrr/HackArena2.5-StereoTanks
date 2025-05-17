namespace GameClient.Scenes.Replay.MatchResultsCore;

#if STEREO

/// <summary>
/// Represents the match results team.
/// </summary>
/// <param name="TeamName">The name of the team.</param>
/// <param name="Color">The color of the team.</param>
/// <param name="RoundsWon">The count of rounds won by the team.</param>
internal record class MatchResultsTeam(
    string TeamName,
    uint Color,
    int RoundsWon);

#endif
