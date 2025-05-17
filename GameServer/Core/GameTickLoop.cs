using System.Diagnostics;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Runs the main game tick loop.
/// </summary>
internal sealed class GameTickLoop(GameInstance game, ILogger logger)
{
#if HACKATHON
    // Used to shuffle the bot actions.
    private readonly Random random = new(game.Settings.Seed);
#endif

    private int tick;

    /// <summary>
    /// Event triggered with each broadcasted state ID.
    /// </summary>
    public event Action<string>? OnBroadcastState;

    /// <summary>
    /// Starts the tick loop asynchronously.
    /// </summary>
    /// <param name="shouldContinue">Predicate to determine if the loop continues.</param>
    /// <param name="onCompleted">Callback when the loop ends.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RunAsync(Func<bool> shouldContinue, Action onCompleted)
    {
        await PreciseTimer.PreciseDelay(game.Settings.BroadcastInterval);

        var stopwatch = new Stopwatch();

        while (shouldContinue())
        {
            if (this.tick++ >= game.Settings.Ticks)
            {
                logger.Verbose("Max ticks reached.");
                onCompleted();
                return;
            }

            stopwatch.Restart();

            lock (game)
            {
#if HACKATHON
                var actionList = game.PacketHandler.HackathonBotActions.Select(kvp => kvp).ToList();
                actionList.Sort((x, y) => x.Key.Instance.Id.CompareTo(y.Key.Instance.Id));
                var actions = actionList.Select(x => x.Value).ToArray();
                this.random.Shuffle(actions);

                foreach (var action in actions)
                {
                    action();
                }

                game.PacketHandler.HackathonBotActions.Clear();
#endif

                try
                {
                    game.StateUpdater.Update(1f);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Logic update failed");
                    game.IsValid = false;
                }

                string stateId = Guid.NewGuid().ToString();
                this.OnBroadcastState?.Invoke(stateId);

                foreach (var player in game.Players)
                {
                    player.ResetGameTickProperties();
                }

                var broadcast = this.BroadcastGameStateAsync(stateId);
                game.ReplayManager?.AddGameState(this.tick, stateId);

                game.StateUpdater.ResetAbilities();
            }

            var sleepTime = (int)(game.Settings.BroadcastInterval - stopwatch.ElapsedMilliseconds);
            logger.Verbose("Sleep time: {SleepTime} ms", sleepTime);

#if HACKATHON
            var tcs = new TaskCompletionSource<bool>();

            void EagerBroadcast(object? sender, PlayerConnection player)
            {
                lock (player)
                {
                    if (game.Settings.EagerBroadcast
                        && game.Players.All(x => x.IsHackathonBot && x.HasMadeActionToCurrentGameState))
                    {
                        _ = tcs.TrySetResult(true);
                        logger.Verbose("Eager broadcast triggered.");
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
                logger.Warning(
                    "Game state broadcast took longer than expected! " +
                    "Tick: {Tick}, {BroadcastTime}/{BroadcastInterval} ms",
                    this.tick,
                    broadcastTime,
                    broadcastInterval);
            }
        }
    }

    private async Task BroadcastGameStateAsync(string stateId)
    {
        var tasks = new List<Task>();
        var cts = new CancellationTokenSource(game.Settings.BroadcastInterval * 100);

        foreach (var connection in game.Connections)
        {
            if (!connection.IsReadyToReceiveGameState)
            {
                continue;
            }

            var payload = game.PayloadHelper.GetGameStatePayload(connection, this.tick, stateId, out var converters);
            var packet = new ResponsePacket(payload, logger, converters);
            tasks.Add(packet.SendAsync(connection, cts.Token, game));

#if STEREO
            if (connection is PlayerConnection player
                && payload is GameStatePayload.ForPlayer playerPayload)
            {
                player.LastGameStatePayload = playerPayload;
            }
#endif
        }

        await Task.WhenAll(tasks);
    }
}
