using System.Collections.Concurrent;
using System.Net.WebSockets;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the player manager.
/// </summary>
/// <param name="game">The game instance.</param>
internal class PlayerManager(GameInstance game)
{
    private static readonly Random Random = new();

    private static readonly uint[] Colors =
    [
        /* ABGR */
        0xFFFFA600,
        0xFFFF5AF9,
        0xFF1A9BF9,
        0xFF3FD47A,
    ];

    /// <summary>
    /// Gets the players.
    /// </summary>
    public ConcurrentDictionary<WebSocket, Player> Players { get; } = new();

    /// <summary>
    /// Adds a player.
    /// </summary>
    /// <param name="socket">The socket of the player.</param>
    /// <param name="connectionData">The connection data of the player.</param>
    /// <returns>The player instance.</returns>
    public GameLogic.Player AddPlayer(WebSocket socket, PlayerConnectionData connectionData)
    {
        string id;
        do
        {
            id = Guid.NewGuid().ToString();
        } while (this.Players.Values.Any(p => p.Instance.Id == id));

        var color = this.GetPlayerColor();

        var instance = new GameLogic.Player(id, connectionData.Nickname, color);
        _ = game.Grid.GenerateTank(instance);

        var player = new Player(instance, connectionData);
        this.Players[socket] = player;

        return instance;
    }

    /// <summary>
    /// Removes a player.
    /// </summary>
    /// <param name="socket">The socket of the player.</param>
    public void RemovePlayer(WebSocket socket)
    {
        var player = this.Players[socket];
        _ = this.Players.Remove(socket, out _);
        _ = game.Grid.RemoveTank(player.Instance);
    }

    /// <summary>
    /// Pings a player in a loop.
    /// </summary>
    /// <param name="socket">The socket of the player.</param>
    /// <param name="cancellationToken">The cancellation token that can be used to cancel the ping loop.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PingPlayerLoop(WebSocket socket, CancellationToken cancellationToken)
    {
        const int pingInterval = 1000;
        var player = this.Players[socket];

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var packet = new EmptyPayload() { Type = PacketType.Ping };
                await game.SendPlayerPacketAsync(socket, packet);
                player.LastPingSentTime = DateTime.UtcNow;

                await Task.Delay(pingInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PingPlayerLoop: {ex.Message}");

                // A small delay to prevent tight loop in case of persistent errors
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    private uint GetPlayerColor()
    {
        foreach (uint color in Colors)
        {
            if (!this.Players.Values.Any(p => p.Instance.Color == color))
            {
                return color;
            }
        }

        return (uint)((0xFF << 24) | Random.Next(0xFFFFFF));
    }
}
