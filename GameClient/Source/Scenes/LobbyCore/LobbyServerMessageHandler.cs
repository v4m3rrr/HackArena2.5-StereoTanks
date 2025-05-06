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

#if STEREO
        updater.UpdateTeamSlotPanels(data.Teams);
#else
        updater.UpdatePlayerSlotPanels(data.Players, data.ServerSettings.NumberOfPlayers);
#endif
    }
}
