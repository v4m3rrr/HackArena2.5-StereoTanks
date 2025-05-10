using CommandLine;
using GameLogic;

namespace GameServer;

#pragma warning disable CS8618
#pragma warning disable SA1118

/// <summary>
/// Represents the command line options.
/// </summary>
internal class CommandLineOptions
{
    /// <summary>
    /// Gets the host to listen on.
    /// </summary>
    [Option(
        'h',
        "host",
        Required = false,
        HelpText = "The host to listen on.",
        Default = "localhost")]
    public string Host { get; private set; }

    /// <summary>
    /// Gets the port to listen on.
    /// </summary>
    [Option(
        'p',
        "port",
        Required = false,
        HelpText = "The port to listen on.",
        Default = 5000)]
    public int Port { get; private set; }

    /// <summary>
    /// Gets the number of players.
    /// </summary>
    [Option(
        'n',
        "number-of-players",
        Required = false,
        HelpText = "The number of players.",
        Default = 4)]
    public int NumberOfPlayers { get; private set; }

    /// <summary>
    /// Gets the grid dimension.
    /// </summary>
    [Option(
        'd',
        "grid-dimension",
        Required = false,
        HelpText = "The dimension of the grid.",
#if STEREO
        Default = 20)]
#else
        Default = 24)]
#endif
    public int GridDimension { get; private set; }

    /// <summary>
    /// Gets the number of ticks per game.
    /// </summary>
    [Option(
        't',
        "ticks",
        Required = false,
        HelpText = "The number of ticks per game.",
        Default = 3000)]
    public int Ticks { get; private set; }

    /// <summary>
    /// Gets the broadcast interval in milliseconds.
    /// </summary>
    [Option(
        'b',
        "broadcast-interval",
        Required = false,
        HelpText = "The broadcast interval in milliseconds.",
        Default = 100)]
    public int BroadcastInterval { get; private set; }

    /// <summary>
    /// Gets the code to join a game.
    /// </summary>
    [Option(
        'c',
        "join-code",
        Required = false,
        HelpText = "The code to join a game.")]
    public string? JoinCode { get; private set; } = null;

    /// <summary>
    /// Gets or sets the seed to use for random number generation.
    /// </summary>
    [Option(
        's',
        "seed",
        Required = false,
        HelpText = "The seed to use for random number generation.")]
    public int? Seed { get; set; }

    /// <summary>
    /// Gets a value indicating whether to run the game in sandbox mode.
    /// </summary>
    [Option(
        'x',
        "sandbox",
        Required = false,
        HelpText = "Whether to run the game in sandbox mode.",
        Default = false)]
    public bool SandboxMode { get; private set; }

    /// <summary>
    /// Gets the timeout duration in milliseconds to wait for a pong response from a player.
    /// </summary>
    /// <remarks>
    /// If a player does not respond with a pong within this timeout after the first ping,
    /// a second ping is sent. If no pong is received within the same duration after the second ping,
    /// the player is removed from the game.
    /// </remarks>
    [Option(
        'k',
        "no-pong-timeout",
        Required = false,
        HelpText = "Timeout duration in milliseconds to wait for a pong from a player. " +
            "If not received within this time after the first ping, a second ping is sent. " +
            "If still not received, the player is removed from the game.",
        Default = 10_000)]
    public int NoPongTimeout { get; private set; }

#if HACKATHON

    /// <summary>
    /// Gets a value indicating whether to broadcast the game state eagerly.
    /// </summary>
    /// <remarks>
    /// If all players return their next move in broadcast interval,
    /// the game state is broadcasted eagerly.
    /// </remarks>
    [Option(
        'e',
        "eager-broadcast",
        Required = false,
        HelpText = "Whether to broadcast the game state eagerly "
            + "(if all players return their next move in broadcast interval).",
        Default = false)]
    public bool EagerBroadcast { get; private set; }

    /// <summary>
    /// Gets the name of the match.
    /// </summary>
    /// <value>
    /// The name of the match if specified;
    /// otherwise, <see langword="null"/>.
    /// </value>
    [Option(
        'm',
        "match-name",
        Required = false,
        HelpText = "The name of the match.",
        Default = null)]
    public string? MatchName { get; private set; }

#endif

    /// <summary>
    /// Gets a value indicating whether to save the replay.
    /// </summary>
    [Option(
        'r',
        "save-replay",
        Required = false,
        HelpText = "Whether to save the replay.",
        Default = false)]
    public bool SaveReplay { get; private set; }

    /// <summary>
    /// Gets the filepath to save the replay to.
    /// </summary>
    [Option(
        "replay-filepath",
        Required = false,
        HelpText = "The filepath to save the replay to.",
        Default = null)]
    public string? ReplayFilepath { get; private set; }

#if HACKATHON

    /// <summary>
    /// Gets a value indicating whether to save the results.
    /// </summary>
    [Option(
        "save-results",
        Required = false,
        HelpText = "Whether to save the results.",
        Default = false)]
    public bool SaveResults { get; private set; }

#endif

    /// <summary>
    /// Gets a value indicating whether to overwrite
    /// the replay file if it already exists.
    /// </summary>
    [Option(
        "overwrite-replay-file",
        Required = false,
        HelpText = "Whether to overwrite the replay file if it already exists.",
        Default = false)]
    public bool OverwriteReplayFile { get; private set; }

#if DEBUG

    /// <summary>
    /// Gets a value indicating whether to skip
    /// validation of the command line options.
    /// </summary>
    [Option(
        "skip-validation",
        Required = false,
        HelpText = "Whether to skip validation of the command line options.",
        Default = false)]
    public bool SkipValidation { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to log packets.
    /// </summary>
    [Option(
        "log-packets",
        Required = false,
        HelpText = "Whether to log packets.",
        Default = false)]
    public bool LogPackets { get; private set; }

#endif

    /// <summary>
    /// Gets a value indicating whether to
    /// skip validation of the host regex.
    /// </summary>
    [Option(
        "skip-host-regex-validation",
        Required = false,
        HelpText = "Whether to skip validation of the host regex.",
        Default = false)]
    public bool SkipHostRegexValidation { get; private set; }
}
