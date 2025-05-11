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

#if HACKATHON
        DebugConsole.SendMessage("Eager broadcast: " + data.ServerSettings.EagerBroadcast, Color.DarkGray);
#endif

        DebugConsole.SendMessage("Game ticks: " + data.ServerSettings.Ticks, Color.DarkGray);
        DebugConsole.SendMessage("Game time: " + (data.ServerSettings.Ticks / (1000f / data.ServerSettings.BroadcastInterval)) + "s", Color.DarkGray);

        Game.Settings = data.ServerSettings;
        Game.PlayerId = data.PlayerId;

#if HACKATHON

        string? matchName = data.ServerSettings.MatchName;
        if (data.ServerSettings.SandboxMode)
        {
            if (matchName is null)
            {
                matchName = "sandbox";
            }
            else
            {
                matchName += " [sandbox]";
            }
        }

        updater.UpdateMatchName(matchName);

#endif

        updater.EnableGridComponent();
    }

    /// <summary>
    /// Handles the game state packet.
    /// </summary>
    /// <param name="packet">The packet containing the game state payload.</param>
    /// <param name="updater">The game updater.</param>
    public static void HandleGameStatePacket(Packet packet, GameUpdater updater)
    {
        bool isSpectator = ServerConnection.Data.IsSpectator;

        GameSerializationContext serializationContext = isSpectator
            ? new GameSerializationContext.Spectator()
            : new GameSerializationContext.Player(Game.PlayerId!)
            {
#if STEREO
                PlayerTeamMap = GameStatePayload.ForPlayer.GetPlayerTeamMap(packet.Payload),
#endif
            };

        var converters = GameStatePayload.GetConverters(serializationContext);
        var serializer = PacketSerializer.GetSerializer(converters);

        var gameState = isSpectator
            ? packet.GetPayload<GameStatePayload>(serializer)
            : packet.GetPayload<GameStatePayload.ForPlayer>(serializer);

#if STEREO
        updater.UpdateTeams(gameState.Teams);
#else
        updater.UpdatePlayers(gameState.Players);
#endif

        updater.UpdateGrid(gameState);

#if STEREO
        updater.UpdateTeams(gameState.Teams);
        updater.RefreshTeamBarPanels();
#else
        updater.UpdatePlayers(gameState.Players);
        updater.RefreshPlayerBarPanels();
#endif

        updater.UpdateTimer(gameState.Tick);
        updater.EnableGridComponent();
    }

    /// <summary>
    /// Handles the game end packet.
    /// </summary>
    /// <param name="packet">The packet containing the game end payload.</param>
    public static void HandleGameEndPacket(Packet packet)
    {
        var converters = GameEndPayload.GetConverters();
        var serializers = PacketSerializer.GetSerializer(converters);
        var payload = packet.GetPayload<GameEndPayload>(serializers);
#if STEREO
        var args = new GameEndDisplayEventArgs(payload.Teams);
#else
        var args = new GameEndDisplayEventArgs(payload.Players);
#endif
        Scene.Change<GameEnd>(args);
    }
}
