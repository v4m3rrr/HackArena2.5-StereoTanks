using Serilog;

namespace GameServer;

/// <summary>
/// Resolves the file path for saving game replays.
/// </summary>
internal static class ReplayPathResolver
{
    /// <summary>
    /// Resolves the replay file path based on options.
    /// </summary>
    /// <param name="options">The command line options.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The resolved file path, or <see langword="null"/> if replay is disabled.</returns>
    public static string? Resolve(CommandLineOptions options, ILogger logger)
    {
        if (!options.SaveReplay)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(options.ReplayFilepath))
        {
            return Path.GetFullPath(options.ReplayFilepath);
        }

        const string replayDir = "Replays";
        if (!Directory.Exists(replayDir))
        {
            logger.Information("Creating replay directory...");
            Directory.CreateDirectory(replayDir);
        }

#if WINDOWS
        const string extension = ".zip";
#else
        const string extension = ".tar.gz";
#endif

        return Path.Combine(replayDir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}{extension}");
    }
}
