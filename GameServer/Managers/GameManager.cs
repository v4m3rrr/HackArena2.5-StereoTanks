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
        if (this.Status is GameStatus.Running)
        {
            return;
        }

        this.Status = GameStatus.Running;

        foreach (var player in game.PlayerManager.Players.Keys)
        {
            var packet = new EmptyPayload() { Type = PacketType.GameStart };
            _ = game.SendPlayerPacketAsync(player, packet);
        }

        _ = Task.Run(this.StartBroadcastingAsync);
    }

    private async Task StartBroadcastingAsync()
    {
        while (true)
        {
            var startTime = DateTime.UtcNow;
            var grid = game.Grid;

            // Update game logic
            grid.UpdateBullets(1f);
            grid.RegeneratePlayersBullets();
            grid.UpdateTanksRegenerationProgress();
            grid.UpdatePlayersVisibilityGrids();
            grid.UpdateZones();

            // Broadcast the game state
            this.ResetAlreadyMovement();
            await this.BroadcastGameStateAsync();

            var endTime = DateTime.UtcNow;
            var sleepTime = game.Settings.BroadcastInterval - (endTime - startTime).Milliseconds;

            if (sleepTime > 0)
            {
                await Task.Delay(sleepTime);
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

    private async Task BroadcastGameStateAsync()
    {
        int currentTick = this.tick++;

        var players = game.PlayerManager.Players.ToDictionary(x => x.Key, x => x.Value.Instance);
        var clients = game.PlayerManager.Players.Keys.Concat(game.SpectatorManager.Spectators.Keys).ToList();

        foreach (WebSocket client in clients)
        {
            GameStatePayload packet;
            GameSerializationContext context;
            SerializationOptions options;

            if (game.SpectatorManager.IsSpectator(client))
            {
                packet = new GameStatePayload(currentTick, [.. players.Values], game.Grid);
                context = new GameSerializationContext.Spectator();
                options = SerializationOptions.Default;
            }
            else
            {
                var player = players[client];
                packet = new GameStatePayload.ForPlayer(currentTick, player, [.. players.Values], game.Grid);
                context = new GameSerializationContext.Player(player);
                var connectionData = game.PlayerManager.Players[client].ConnectionData;
                options = new SerializationOptions() { TypeOfPacketType = connectionData.TypeOfPacketType };
            }

            var converters = GameStatePayload.GetConverters(context);
            var buffer = PacketSerializer.ToByteArray(packet, converters, options);

            if (client.State == WebSocketState.Open)
            {
                try
                {
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while broadcasting game state: {ex.Message}");
                }
            }
        }
    }
}
