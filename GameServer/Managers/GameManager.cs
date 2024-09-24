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
    private int tick = 0;

    /// <summary>
    /// Gets a status of the game.
    /// </summary>
    public GameStatus Status { get; private set; }

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

            // Update game logic
            grid.UpdateBullets(1f);
            grid.RegeneratePlayersBullets();
            grid.UpdateTanksRegenerationProgress();
            grid.UpdatePlayersVisibilityGrids();
            grid.UpdateZones();

            // Broadcast the game state
            this.ResetAlreadyMovement();
            var broadcast = this.BroadcastGameStateAsync();
            await Task.WhenAll(broadcast);

            stopwatch.Stop();

            var sleepTime = (int)(game.Settings.BroadcastInterval - stopwatch.ElapsedMilliseconds);
            if (sleepTime > 0)
            {
                await PreciseTimer.PreciseDelay(sleepTime);
            }
            else
            {
                Console.WriteLine("Game state broadcast took longer than expected!");
            }
        }
    }

    private void ResetAlreadyMovement()
    {
        foreach (Player player in game.PlayerManager.Players.Values)
        {
            lock (player)
            {
                player.HasMadeMovementThisTick = false;
            }
        }
    }

    private List<Task> BroadcastGameStateAsync()
    {
        var tasks = new List<Task>();

        var players = game.PlayerManager.Players.ToDictionary(x => x.Key, x => x.Value.Instance);
        var clients = game.PlayerManager.Players.Keys.Concat(game.SpectatorManager.Spectators.Keys).ToList();

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
                packet = new GameStatePayload.ForPlayer(this.tick, player, [.. players.Values], game.Grid);
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
