using System.Collections.Generic;
using GameClient.LobbySceneComponents;
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
    /// <param name="numberOfPlayers">The maximum number of players in the game.</param>
    public void UpdatePlayerSlotPanels(List<Player> player, int numberOfPlayers)
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            components.PlayerSlotPanels[i].IsEnabled = i < numberOfPlayers;
        }

        for (int i = 0; i < player.Count; i++)
        {
            components.PlayerSlotPanels[i].Player = player[i];
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
