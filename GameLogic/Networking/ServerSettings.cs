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
/// <param name="EagerBroadcast">Whether to broadcast the game state eagerly.</param>
/// <param name="MatchName">The name of the match.</param>
/// <remarks>
/// <para>
/// <paramref name="Ticks"/> are <see langword="null"/>
/// when the game sandbox mode is enabled.
/// </para>
/// <para>
/// <paramref name="EagerBroadcast"/> and <paramref name="MatchName"/>
/// are only available in HACKATHON builds.
/// </para>
/// </remarks>
public record class ServerSettings(
    int GridDimension,
    int NumberOfPlayers,
    int Seed,
    int? Ticks,
    int BroadcastInterval,
    bool SandboxMode,
    bool EagerBroadcast,
    string? MatchName);
