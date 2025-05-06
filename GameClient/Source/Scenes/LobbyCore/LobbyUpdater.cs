using System.Collections.Generic;
using GameClient.Networking;
using GameClient.UI.LobbySceneComponents;
using GameLogic;

namespace GameClient.Scenes.LobbyCore;

/// <summary>
/// Represents the lobby updater.
/// </summary>
/// <param name="components">The lobby components.</param>
internal class LobbyUpdater(LobbyComponents components)
{
#if STEREO

    /// <summary>
    /// Updates the team slot panels.
    /// </summary>
    /// <param name="teams">The list of teams.</param>
    public void UpdateTeamSlotPanels(List<Team> teams)
    {
        for (int i = 0; i < components.TeamSlotPanels.Count; i++)
        {
            components.TeamSlotPanels[i].Team = i < teams.Count ? teams[i] : null;
        }
    }

#else

    /// <summary>
    /// Updates the player slot panels.
    /// </summary>
    /// <param name="players">The list of players.</param>
    /// <param name="numberOfPlayers">The maximum number of players in the game.</param>
    public void UpdatePlayerSlotPanels(List<Player> players, int numberOfPlayers)
    {
        for (int i = 0; i < components.PlayerSlotPanels.Count; i++)
        {
            components.PlayerSlotPanels[i].IsEnabled = i < numberOfPlayers;
            components.PlayerSlotPanels[i].Player = i < players.Count ? players[i] : null;
        }
    }

#endif

    /// <summary>
    /// Updates the join code.
    /// </summary>
    public void UpdateJoinCode()
    {
        string? joinCode = ServerConnection.Data.JoinCode;
        components.JoinCode.IsEnabled = !string.IsNullOrEmpty(joinCode);
        components.JoinCode.Value = joinCode ?? "-";
    }

#if HACKATHON

    /// <summary>
    /// Updates the match name.
    /// </summary>
    /// <param name="matchName">The match name to set.</param>
    public void UpdateMatchName(string? matchName)
    {
        components.MatchName.Value = matchName ?? "Lobby";
    }

#endif

#if STEREO

    /// <summary>
    /// Resets the team slot panels.
    /// </summary>
    public void ResetTeamSlotPanels()
    {
        foreach (var panel in components.TeamSlotPanels)
        {
            panel.IsEnabled = false;
            panel.Team = null;
        }
    }

#else

    /// <summary>
    /// Resets the player slot panels.
    /// </summary>
    public void ResetPlayerSlotPanels()
    {
        foreach (var panel in components.PlayerSlotPanels)
        {
            panel.IsEnabled = false;
            panel.Player = null;
        }
    }

#endif
}
