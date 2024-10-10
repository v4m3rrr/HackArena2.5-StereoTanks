using System.Diagnostics;
using GameLogic;

namespace GameServer;

/// <summary>
/// Represents the game manager.
/// </summary>
/// <param name="game">The game instance.</param>
internal class GameManager(GameInstance game)
{
#if HACKATHON
    // Used to shuffle the bot actions.
    private readonly Random random = new(game.Settings.Seed);
#endif

    private readonly LogicUpdater logicUpdater = new(game.Grid);
    private int tick = 0;

    /// <summary>
    /// Gets a status of the game.
    /// </summary>
    public GameStatus Status { get; private set; }

    /// <summary>
    /// Gets the current game state id.
    /// </summary>
    public string? CurrentGameStateId { get; private set; }

    /// <summary>
    /// Starts the game.
    /// </summary>
    public async void StartGame()
    {
        lock (this)
        {
            if (this.Status is GameStatus.Running)
            {
                return;
            }

            this.Status = GameStatus.Running;
        }

        game.ReplayManager?.SaveLobbyData();

        await game.LobbyManager.SendGameStartToAll();
        _ = Task.Run(this.StartBroadcastingAsync);
    }

    /// <summary>
    /// Ends the game.
    /// </summary>
    public async void EndGame()
    {
        this.Status = GameStatus.Ended;

        var tasks = new List<Task>();

        foreach (var connection in game.Connections)
        {
            var payload = game.PayloadHelper.GetGameEndPayload(out var converters);
            var packet = new ResponsePacket(payload, converters);
            var task = Task.Run(async () =>
            {
                await packet.SendAsync(connection);
                await connection.CloseAsync(description: "Game ended");
            });
            tasks.Add(task);
        }

        game.ReplayManager?.SaveGameEnd();
        game.ReplayManager?.SaveReplay();

#if HACKATHON
        game.ReplayManager?.SaveResults();
#endif

        await Task.WhenAll(tasks);
        Environment.Exit(0);
    }

    private async Task StartBroadcastingAsync()
    {
        // Give some time for the clients to load the game
        await PreciseTimer.PreciseDelay(game.Settings.BroadcastInterval);

        var stopwatch = new Stopwatch();

        while (this.Status is GameStatus.Running)
        {
            if (this.tick++ >= game.Settings.Ticks)
            {
                this.EndGame();
                break;
            }

            stopwatch.Restart();

#if HACKATHON
            var actionList = game.PacketHandler.HackathonBotActions.ToList();
            actionList.Sort((x, y) => x.Key.Instance.Nickname.CompareTo(y.Key.Instance.Nickname));
            Action[] actions = actionList.Select(x => x.Value).ToArray();
            this.random.Shuffle(actions);

            foreach (Action action in actions)
            {
                action.Invoke();
            }

            game.PacketHandler.HackathonBotActions.Clear();
#endif

            this.logicUpdater.UpdateGrid();

            // Broadcast the game state
            lock (this.CurrentGameStateId ?? new object())
            {
                this.CurrentGameStateId = Guid.NewGuid().ToString();
            }

            foreach (PlayerConnection player in game.Players)
            {
                player.ResetGameTickProperties();
            }

            var broadcast = this.BroadcastGameStateAsync();
            game.ReplayManager?.AddGameState(this.tick, this.CurrentGameStateId);

            await Task.WhenAll(broadcast);
            stopwatch.Stop();

            var sleepTime = (int)(game.Settings.BroadcastInterval - stopwatch.ElapsedMilliseconds);

#if HACKATHON
            var tcs = new TaskCompletionSource<bool>();

            void EagerBroadcast(object? sender, PlayerConnection player)
            {
                lock (player)
                {
                    if (this.tick > 5 // Warm-up period
                        && game.Settings.EagerBroadcast
                        && game.Players.All(x => x.IsHackathonBot && x.HasMadeActionToCurrentGameState))
                    {
                        _ = tcs.TrySetResult(true);
                    }
                }
            }

            if (sleepTime > 0)
            {
                game.PacketHandler.HackathonBotMadeAction += EagerBroadcast;
                var delayTask = PreciseTimer.PreciseDelay(sleepTime);
                await Task.WhenAny(delayTask, tcs.Task);
                game.PacketHandler.HackathonBotMadeAction -= EagerBroadcast;
            }
#else
            if (sleepTime > 0)
            {
                await PreciseTimer.PreciseDelay(sleepTime);
            }
#endif
            else
            {
                var broadcastTime = stopwatch.ElapsedMilliseconds;
                var broadcastInterval = game.Settings.BroadcastInterval;
                Console.WriteLine(
                    "[WARN] Game state broadcast took longer than expected! " +
                    $"[^^^^] Tick {this.tick}, {broadcastTime}/{broadcastInterval} ms");
            }
        }
    }

    private List<Task> BroadcastGameStateAsync()
    {
        var tasks = new List<Task>();

        foreach (Connection connection in game.Connections)
        {
            var payload = game.PayloadHelper.GetGameStatePayload(
                connection,
                this.tick,
                this.CurrentGameStateId!,
                out var converters);

            var packet = new ResponsePacket(payload, converters);
            var task = packet.SendAsync(connection);
            tasks.Add(task);
        }

        return tasks;
    }
}
