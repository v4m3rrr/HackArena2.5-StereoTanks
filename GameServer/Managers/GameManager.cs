using System.Diagnostics;
using System.Net.WebSockets;
using GameLogic.Networking;

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
    public void StartGame()
    {
        lock (this)
        {
            if (this.Status is GameStatus.Running)
            {
                return;
            }

            this.Status = GameStatus.Running;
        }

        foreach (var player in game.PlayerManager.Players.Keys)
        {
            var packet = new EmptyPayload() { Type = PacketType.GameStart };
            _ = game.SendPlayerPacketAsync(player, packet);
        }

        _ = Task.Run(this.StartBroadcastingAsync);
    }

    /// <summary>
    /// Ends the game.
    /// </summary>
    public void EndGame()
    {
        this.Status = GameStatus.Ended;

        var players = game.PlayerManager.Players.Values.Select(x => x.Instance).ToList();
        players.Sort((x, y) => y.Score.CompareTo(x.Score));

        var packet = new GameEndPayload(players);
        var converters = GameEndPayload.GetConverters();

        foreach (var player in game.PlayerManager.Players.Keys)
        {
            _ = game.SendPlayerPacketAsync(player, packet, converters);
        }

        foreach (var spectator in game.SpectatorManager.Spectators.Keys)
        {
            _ = game.SendSpectatorPacketAsync(spectator, packet, converters);
        }

        var clients = game.PlayerManager.Players.Keys.Concat(game.SpectatorManager.Spectators.Keys).ToList();

        foreach (var client in clients)
        {
            _ = client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Game ended", CancellationToken.None);
        }

        Environment.Exit(0);
    }

    private async Task StartBroadcastingAsync()
    {
        var stopwatch = new Stopwatch();

        while (this.Status is GameStatus.Running)
        {
            if (this.tick++ >= game.Settings.Ticks)
            {
                this.EndGame();
                break;
            }

            stopwatch.Restart();

            var grid = game.Grid;

#if HACKATHON
            var botActions = game.PacketHandler.HackathonBotActions;
            var actionList = botActions.ToList();
            actionList.Sort((x, y) => x.Key.Instance.Nickname.CompareTo(y.Key.Instance.Nickname));
            Action[] actions = actionList.Select(x => x.Value).ToArray();
            this.random.Shuffle(actions);

            foreach (Action action in actions)
            {
                action.Invoke();
            }

            game.PacketHandler.HackathonBotActions.Clear();
#endif

            // Update game logic
            grid.UpdateBullets(1f);
            grid.RegeneratePlayersBullets();
            grid.UpdateTanksRegenerationProgress();
            grid.UpdatePlayersVisibilityGrids();
            grid.UpdateZones();

            // Broadcast the game state
            this.ResetPlayerGameTickProperties();
            var broadcast = this.BroadcastGameStateAsync();
            await Task.WhenAll(broadcast);

            stopwatch.Stop();

            var sleepTime = (int)(game.Settings.BroadcastInterval - stopwatch.ElapsedMilliseconds);

#if HACKATHON
            var tcs = new TaskCompletionSource<bool>();

            void Func(dynamic? s, dynamic e)
            {
                lock (e)
                {
                    if (this.tick > 5 && game.Settings.EagerBroadcast && game.PlayerManager.Players.Values.All(x => x.IsHackathonBot))
                    {
                        var alivePlayers = game.PlayerManager.Players.Values.Where(x => !x.Instance.IsDead);
                        if (alivePlayers.All(x => x.HasMadeActionToCurrentGameState))
                        {
                            _ = tcs.TrySetResult(true);
                        }
                    }
                }
            }

            game.PacketHandler.HackathonBotMadeAction += Func;

            if (sleepTime > 0)
            {
                var delayTask = PreciseTimer.PreciseDelay(sleepTime);
                var completedTask = await Task.WhenAny(delayTask, tcs.Task);
                if (completedTask == tcs.Task)
                {
                    Console.WriteLine("All alive players returned their move, broadcasting early.");
                }
                else if (game.Settings.EagerBroadcast)
                {
                    Console.WriteLine("Not all alive players returned their move, broadcasting normally.");
                }
            }
#else
            if (sleepTime > 0)
            {
                await PreciseTimer.PreciseDelay(sleepTime);
            }
#endif
            else
            {
                Console.WriteLine("Game state broadcast took longer than expected!");
            }

#if HACKATHON
            game.PacketHandler.HackathonBotMadeAction -= Func;
#endif
        }
    }

    private void ResetPlayerGameTickProperties()
    {
        foreach (Player player in game.PlayerManager.Players.Values)
        {
            lock (player)
            {
                player.HasMadeActionThisTick = false;
#if HACKATHON
                player.HasMadeActionToCurrentGameState = false;
#endif
            }
        }
    }

    private List<Task> BroadcastGameStateAsync()
    {
        var tasks = new List<Task>();

        var players = game.PlayerManager.Players.ToDictionary(x => x.Key, x => x.Value.Instance);
        var clients = game.PlayerManager.Players.Keys.Concat(game.SpectatorManager.Spectators.Keys).ToList();

        lock (this.CurrentGameStateId ?? new object())
        {
            this.CurrentGameStateId = Guid.NewGuid().ToString();
        }

        foreach (WebSocket client in clients)
        {
            GameStatePayload packet;
            GameSerializationContext context;

            if (game.SpectatorManager.IsSpectator(client))
            {
                packet = new GameStatePayload(this.tick, [.. players.Values], game.Grid);
                context = new GameSerializationContext.Spectator();
            }
            else
            {
                var player = players[client];
                packet = new GameStatePayload.ForPlayer(this.CurrentGameStateId, this.tick, player, [.. players.Values], game.Grid);
                context = new GameSerializationContext.Player(player);
            }

            var converters = GameStatePayload.GetConverters(context);

            if (client.State == WebSocketState.Open)
            {
                var task = Task.Run(() => game.SendPacketAsync(client, packet, converters));
                tasks.Add(task);
            }
        }

        return tasks;
    }
}
