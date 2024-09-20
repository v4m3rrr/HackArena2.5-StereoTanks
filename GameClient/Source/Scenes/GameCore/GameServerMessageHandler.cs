using System.Net.WebSockets;
using GameClient.Networking;
using GameLogic.Networking;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes.GameCore;

/// <summary>
/// Represents a game server message handler.
/// </summary>
internal static class GameServerMessageHandler
{
    /// <summary>
    /// Handles the close message.
    /// </summary>
    /// <param name="result">The result of the close message.</param>
    public static async void HandleCloseMessage(WebSocketReceiveResult result)
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
        await ServerConnection.CloseAsync();
    }

    /// <summary>
    /// Handles the ping packet.
    /// </summary>
    /// <remarks>
    /// Sends a pong packet back to the server.
    /// </remarks>
    public static async void HandlePingPacket()
    {
        var pong = new EmptyPayload() { Type = PacketType.Pong };
        await ServerConnection.SendAsync(PacketSerializer.Serialize(pong));
    }

    /// <summary>
    /// Handles the game data packet.
    /// </summary>
    /// <param name="packet">The packet containing the game data payload.</param>
    /// <param name="updater">The game updater.</param>
    public static void HandleLobbyDataPacket(Packet packet, GameUpdater updater)
    {
        var converters = LobbyDataPayload.GetConverters();
        var serializers = PacketSerializer.GetSerializer(converters);
        var data = packet.GetPayload<LobbyDataPayload>(serializers);

        DebugConsole.SendMessage("Broadcast interval: " + data.ServerSettings.BroadcastInterval + "ms", Color.DarkGray);
        DebugConsole.SendMessage("Player ID: " + data.PlayerId, Color.DarkGray);
        DebugConsole.SendMessage("Seed: " + data.ServerSettings.Seed, Color.DarkGray);
        DebugConsole.SendMessage("Eager broadcast: " + data.ServerSettings.EagerBroadcast, Color.DarkGray);

        Game.PlayerId = data.PlayerId;
        Game.ServerBroadcastInterval = data.ServerSettings.BroadcastInterval;

        updater.EnableGridComponent();
    }

    /// <summary>
    /// Handles the game state packet.
    /// </summary>
    /// <param name="packet">The packet containing the game state payload.</param>
    /// <param name="updater">The game updater.</param>
    public static void HandleGameStatePacket(Packet packet, GameUpdater updater)
    {
        var isSpectator = ServerConnection.Data.IsSpectator;

        GameSerializationContext context = isSpectator
            ? new GameSerializationContext.Spectator()
            : new GameSerializationContext.Player(Game.PlayerId!);

        var converters = GameStatePayload.GetConverters(context);
        var serializer = PacketSerializer.GetSerializer(converters);

        GameStatePayload gameState = isSpectator
            ? packet.GetPayload<GameStatePayload>(serializer)
            : packet.GetPayload<GameStatePayload.ForPlayer>(serializer);

        updater.UpdateTimer(gameState.Tick);
        updater.UpdateGridLogic(gameState);
        updater.UpdatePlayers(gameState.Players);
        updater.RefreshPlayerBarPanels();

        if (gameState is GameStatePayload.ForPlayer playerGameState)
        {
            updater.UpdatePlayerFogOfWar(playerGameState);
        }
        else if (isSpectator)
        {
            updater.UpdatePlayersFogOfWar();
        }

        updater.EnableGridComponent();
    }
}
