using System.Net;
using GameLogic;
using Serilog;

namespace GameServer;

/// <summary>
/// Represents the game server host responsible for managing server lifecycle.
/// </summary>
/// <param name="options">The command-line options.</param>
/// <param name="logger">The logger.</param>
internal sealed class GameServerHost(CommandLineOptions options, ILogger logger)
{
    private HttpListener? listener;
    private GameInstance? game;

    /// <summary>
    /// Runs the server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync()
    {
        this.listener = new HttpListener();
        this.listener.Prefixes.Add($"http://{options.Host}:{options.Port}/");
        this.listener.Start();

        logger.Information("Listening on http://{host}:{port}/", options.Host, options.Port);
        this.InitializeGame();

        logger.Information("Press Ctrl+C to stop the server.");

        while (true)
        {
            HttpListenerContext context = await this.listener.GetContextAsync();
            _ = Task.Run(() => new HttpConnectionHandler(context, this.game!, logger).HandleAsync());
        }
    }

    private void InitializeGame()
    {
        options.Seed ??= new Random().Next();

        string? replayPath = ReplayPathResolver.Resolve(options, logger);

        this.game = replayPath is not null
            ? new GameInstance(options, logger, replayPath)
            : new GameInstance(options, logger);

        logger.Information("Generating map...");

        var mapService = new MapGenerationSystem(this.game.Grid);
        mapService.GenerateMap((s, e) => logger.Warning("Map generation: {warning}", e));

        logger.Information("Map generated.");

        if (options.SandboxMode)
        {
            this.game.GameManager.StartGame();
        }
    }
}
