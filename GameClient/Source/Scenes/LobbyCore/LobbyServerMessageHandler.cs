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
    /// <param name="serverSettings">The server settings.</param>
    public static void HandleLobbyDataPacket(Packet packet, LobbyUpdater updater, out ServerSettings serverSettings)
    {
        var converters = LobbyDataPayload.GetConverters();
        var serializers = PacketSerializer.GetSerializer(converters);
        var data = packet.GetPayload<LobbyDataPayload>(serializers);

        serverSettings = data.ServerSettings;

        updater.UpdatePlayerSlotPanels(data.Players);
    }
}
