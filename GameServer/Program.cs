using System.Reflection;
using System.Text;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Entry point for the game server.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry point of the server.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static async Task Main(string[] args)
    {
        var logger = ConfigureLogger();

#if STEREO
        logger.Information("Starting StereoTanks server...");
#else
        logger.Information("Starting MonoTanks server...");
#endif

        logger.Information("Version: {version}", GetVersion());

        CommandLineOptions? options = CommandLineParser.Parse(args, logger);
        if (options is null)
        {
            logger.Error("Failed to parse command-line options.");
            return;
        }

#if HACKATHON
        logger.Information("Hackathon mode enabled.");
#endif

        logger.Information("Number of players: {numberOfPlayers}", options.NumberOfPlayers);
        logger.Information("Grid dimension: {dimension}", options.GridDimension);
        logger.Information("Sandbox mode: {sandboxMode}", options.SandboxMode);
        logger.Information("Ticks: {ticks}", options.Ticks);
        logger.Information("Broadcast interval: {broadcastInterval}", options.BroadcastInterval);

#if HACKATHON
        logger.Information("Match name: {matchName}", options.MatchName);
        logger.Information("Eager broadcast: {eagerBroadcast}", options.EagerBroadcast);
#endif

        if (options.LogPackets)
        {
            PacketLogger.Enable();
        }

        PacketSerializer.ExceptionThrew += (e) => logger.Error(e, "Packet serialization error.");

        try
        {
            var host = new GameServerHost(options, logger);
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Unhandled exception in server runtime.");
        }
    }

    /// <summary>
    /// Configures the Serilog logger.
    /// </summary>
    /// <returns>A configured logger instance.</returns>
    private static Serilog.Core.Logger ConfigureLogger()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ffff");
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        return new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss:ffff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File($"logs/{timestamp}.log")
            .CreateLogger();
    }

    private static string GetVersion()
    {
        var assembly = typeof(Program).Assembly;
        var version = assembly.GetName().Version!;

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var sb = new StringBuilder();
        sb.Append('v');

        if (informationalVersion is not null)
        {
#if RELEASE
            sb.Append(informationalVersion.Split('+')[0]);
#else
            sb.Append(informationalVersion);
#endif
        }
        else
        {
            sb.Append(version.Major)
                .Append('.')
                .Append(version.Minor)
                .Append('.')
                .Append(version.Build);
        }

#if WINDOWS
        const string Platform = "Windows";
#elif LINUX
        const string Platform = "Linux";
#elif OSX
        const string Platform = "macOS";
#else
#error Platform not supported.
#endif

        sb.Append(" (")
            .Append(Platform)
            .Append(')');

#if DEBUG
        var configuration = assembly
            .GetCustomAttribute<AssemblyConfigurationAttribute>()!
            .Configuration;

        sb.Append(" [")
            .Append(configuration)
            .Append(']');
#endif

        return sb.ToString();
    }
}
