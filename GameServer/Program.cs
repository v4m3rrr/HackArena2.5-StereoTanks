using System.Reflection;
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

        CommandLineOptions? options = CommandLineParser.Parse(args, logger);
        if (options is null)
        {
            logger.Error("Failed to parse command-line options.");
            return;
        }

#if DEBUG
        if (options.LogPackets)
        {
            PacketLogger.Enable();
        }
#endif

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
}
