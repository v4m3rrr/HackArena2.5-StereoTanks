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
        for (int i = 0; i < components.PlayerSlotPanels.Count; i++)
        {
            components.PlayerSlotPanels[i].IsEnabled = i < numberOfPlayers;
            components.PlayerSlotPanels[i].Player = i < player.Count ? player[i] : null;
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

    /// <summary>
    /// Resets the player slot panels.
    /// </summary>
    public void ResetPlayerSlotPanels()
    {
        foreach (var playerSlotPanel in components.PlayerSlotPanels)
        {
            playerSlotPanel.IsEnabled = false;
            playerSlotPanel.Player = null;
        }
    }
}
