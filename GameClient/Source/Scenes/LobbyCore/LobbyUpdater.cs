using System.Collections.Generic;
using GameClient.Networking;
using GameLogic;

namespace GameClient.Scenes.LobbyCore;

/// <summary>
/// Represents the lobby updater.
/// </summary>
/// <param name="components">The lobby components.</param>
internal class LobbyUpdater(LobbyComponents components)
{
    /// <summary>
    /// Updates the player slot panels.
    /// </summary>
    /// <param name="player">The list of players.</param>
    public void UpdatePlayerSlotPanels(List<Player> player)
    {
        for (int i = 0; i < player.Count; i++)
        {
            components.PlayerInfos[i].Player = player[i];
        }
    }

    /// <summary>
    /// Updates the join code.
    /// </summary>
    public void UpdateJoinCode()
    {
        string? joinCode = ServerConnection.Data.JoinCode;
        components.JoinCode.IsEnabled = !string.IsNullOrEmpty(joinCode);
        components.JoinCode.Value = joinCode ?? "-";
    }
}
