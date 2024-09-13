using System.Net.WebSockets;
using GameClient.Networking;
using GameLogic.Networking;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes.GameCore;

/// <summary>
/// Represents a game server message handler.
/// </summary>
/// <param name="connection">The server connection.</param>
internal class GameServerMessageHandler(ServerConnection connection)
{
    /// <summary>
    /// Handles the close message.
    /// </summary>
    /// <param name="result">The result of the close message.</param>
    public async void HandleCloseMessage(WebSocketReceiveResult result)
    {
        WebSocketCloseStatus? status = result.CloseStatus;
        string? description = result.CloseStatusDescription;

        var msg = description is null
            ? $"Server status: connection closed ({(int?)status ?? -1})"
            : $"Server status: connection closed ({(int?)status ?? -1}) - {description}";

        if (status == WebSocketCloseStatus.NormalClosure)
        {
            DebugConsole.SendMessage(msg);
        }
        else
        {
            DebugConsole.ThrowError(msg);
        }

        Scene.ChangeToPreviousOrDefault<MainMenu>();
        await connection.CloseAsync();
    }

    /// <summary>
    /// Handles the ping packet.
    /// </summary>
    /// <remarks>
    /// Sends a pong packet back to the server.
    /// </remarks>
    public async void HandlePingPacket()
    {
        var pong = new EmptyPayload() { Type = PacketType.Pong };
        await connection.SendAsync(PacketSerializer.Serialize(pong));
    }

    /// <summary>
    /// Handles the game data packet.
    /// </summary>
    /// <param name="packet">The packet containing the game data payload.</param>
    /// <param name="updater">The game updater.</param>
    /// <param name="broadcastInterval">The server broadcast interval.</param>
    public void HandleGameDataPacket(Packet packet, GameUpdater updater, out int broadcastInterval)
    {
        var gameData = packet.GetPayload<GameDataPayload>();

        DebugConsole.SendMessage("Broadcast interval: " + gameData.BroadcastInterval + "ms", Color.DarkGray);
        DebugConsole.SendMessage("Player ID: " + gameData.PlayerId, Color.DarkGray);
        DebugConsole.SendMessage("Seed: " + gameData.Seed, Color.DarkGray);

        updater.UpdatePlayerId(gameData.PlayerId);
        updater.EnableGridComponent();

        broadcastInterval = gameData.BroadcastInterval;
    }

    /// <summary>
    /// Handles the game state packet.
    /// </summary>
    /// <param name="packet">The packet containing the game state payload.</param>
    /// <param name="updater">The game updater.</param>
    public void HandleGameStatePacket(Packet packet, GameUpdater updater)
    {
        var isSpectator = connection.ConnectionData.IsSpectator;

        SerializationContext context = isSpectator
            ? new SerializationContext.Spectator()
            : new SerializationContext.Player(updater.PlayerId!);

        var converters = GameStatePayload.GetConverters(context);
        var serializer = PacketSerializer.GetSerializer(converters);

        GameStatePayload gameState = isSpectator
            ? packet.GetPayload<GameStatePayload>(serializer)
            : packet.GetPayload<GameStatePayload.ForPlayer>(serializer);

        updater.UpdateTimer(gameState.Time);
        updater.UpdateGridLogic(gameState);
        updater.UpdatePlayers(gameState.Players);
        updater.RefreshPlayerBarPanels();

        if (gameState is GameStatePayload.ForPlayer playerGameState)
        {
            updater.UpdatePlayerFogOfWar(playerGameState);
        }

        updater.EnableGridComponent();
    }
}
