using CommandLine;

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
}
