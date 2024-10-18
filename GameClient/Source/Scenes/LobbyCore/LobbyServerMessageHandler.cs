using GameLogic.Networking;

namespace GameClient.Scenes.LobbyCore;

/// <summary>
/// Represents the lobby server message handler.
/// </summary>
internal static class LobbyServerMessageHandler
{
    /// <summary>
    /// Handles the lobby data packet.
    /// </summary>
    /// <param name="packet">The packet containing the lobby data payload.</param>
    /// <param name="updater">The lobby updater.</param>
    public static void HandleLobbyDataPacket(Packet packet, LobbyUpdater updater)
    {
        var converters = LobbyDataPayload.GetConverters();
        var serializers = PacketSerializer.GetSerializer(converters);
        var data = packet.GetPayload<LobbyDataPayload>(serializers);

        var serverSettings = data.ServerSettings;
        Game.ServerBroadcastInterval = serverSettings.BroadcastInterval;
        Game.PlayerId = data.PlayerId;

        updater.UpdatePlayerSlotPanels(data.Players, serverSettings.NumberOfPlayers);
    }
}
