namespace GameLogic.Networking;

/// <summary>
/// Represents the server settings.
/// </summary>
/// <param name="GridDimension">The dimension of the grid.</param>
/// <param name="NumberOfPlayers">The number of players in the game.</param>
/// <param name="Seed">The seed used to generate the grid.</param>
/// <param name="Ticks">The number of ticks per game.</param>
/// <param name="BroadcastInterval">The interval in milliseconds between broadcasts.</param>
/// <param name="SandboxMode">Whether to enable sandbox mode.</param>
/// <param name="Version">The version of the server.</param>
/// <remarks>
/// <para>
/// <paramref name="Ticks"/> are <see langword="null"/>
/// when the game sandbox mode is enabled.
/// </para>
/// </remarks>
public record class ServerSettings(
    int GridDimension,
    int NumberOfPlayers,
    int Seed,
    int? Ticks,
    int BroadcastInterval,
    bool SandboxMode,
    string Version)
{
#if HACKATHON

    /// <summary>
    /// Gets a value indicating whether to broadcast the game state eagerly.
    /// </summary>
    public required bool EagerBroadcast { get; init; }

    /// <summary>
    /// Gets the name of the match.
    /// </summary>
    public required string? MatchName { get; init; }

#endif
}
