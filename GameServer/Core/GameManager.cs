using GameLogic;
using Serilog;

namespace GameServer;

/// <summary>
/// Coordinates the lifecycle of a match.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger.</param>
internal sealed class GameManager(GameInstance game, ILogger logger)
{
    /// <summary>
    /// Gets a status of the game.
    /// </summary>
    public GameStatus Status { get; private set; }

    /// <summary>
    /// Gets the current game state id.
    /// </summary>
    public string? CurrentGameStateId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the game is in progress.
    /// </summary>
    public bool IsInProgess => this.Status is GameStatus.Starting or GameStatus.Running;

    /// <summary>
    /// Starts the game.
    /// </summary>
    public async void StartGame()
    {
        lock (game)
        {
            if (this.Status is GameStatus.Starting or GameStatus.Running)
            {
                return;
            }

            this.Status = GameStatus.Starting;

#if STEREO
            foreach (var player in game.Players)
            {
                if (player.Instance.Tank is DeclaredTankStub stub)
                {
                    game.Systems.Spawn.GenerateTank(stub);
                }
            }
#endif

            logger.Information("Game is starting...");
        }

        game.ReplayManager?.SaveLobbyData();
        await game.LobbyManager.SendGameStartingToAll();

        if (!game.Settings.SandboxMode)
        {
            while (game.Players.Any(x => !x.IsReadyToReceiveGameState))
            {
                await Task.Delay(100);
            }
        }

        _ = game.LobbyManager.SendGameStartedToAll();

        this.Status = GameStatus.Running;
        logger.Information("Game has started!");

        game.TickLoop.OnBroadcastState += id => this.CurrentGameStateId = id;
        _ = game.TickLoop.RunAsync(() => this.Status == GameStatus.Running, this.EndGame);
    }

    /// <summary>
    /// Ends the game.
    /// </summary>
    public async void EndGame()
    {
        this.Status = GameStatus.Ended;

        logger.Information("Game has ended.");

        game.ReplayManager?.SaveGameEnd();
        game.ReplayManager?.SaveReplay();

#if HACKATHON
        if (game.Options.SaveResults)
        {
            game.ReplayManager?.SaveResults();
        }
#endif

        var tasks = new List<Task>();
        var cts = new CancellationTokenSource(10_000);

        foreach (var connection in game.Connections)
        {
            var payload = game.PayloadHelper.GetGameEndPayload(connection, out var converters);
            var packet = new ResponsePacket(payload, logger, converters);
            tasks.Add(packet.SendAsync(connection, cts.Token, this));
        }

        await Task.WhenAll(tasks);

        foreach (var connection in game.Connections)
        {
            tasks.Add(connection.CloseAsync(description: "Game ended"));
        }

        await Task.WhenAll(tasks);

        Environment.Exit(0);
    }
}
