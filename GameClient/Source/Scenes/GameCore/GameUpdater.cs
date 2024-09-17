using System.Collections.Generic;
using System.Linq;
using GameClient.Networking;
using GameLogic;
using GameLogic.Networking;
using Microsoft.Xna.Framework;

namespace GameClient.Scenes.GameCore;

/// <summary>
/// Represents a game updater.
/// </summary>
/// <param name="components">The game screen components.</param>
/// <param name="players">The list of players.</param>
internal class GameUpdater(GameComponents components, Dictionary<string, Player> players)
{
    /// <summary>
    /// Gets the player ID.
    /// </summary>
    /// <remarks>
    /// If client is a spectator, this property is <see langword="null"/>.
    /// </remarks>
    public string? PlayerId { get; private set; }

    /// <summary>
    /// Gets the game screen components.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    public void UpdatePlayerId(string? playerId)
    {
        this.PlayerId = playerId;
    }

    /// <summary>
    /// Enables the grid component.
    /// </summary>
    public void EnableGridComponent()
    {
        components.Grid.IsEnabled = true;
    }

    /// <summary>
    /// Disables the grid component.
    /// </summary>
    public void DisableGridComponent()
    {
        components.Grid.IsEnabled = false;
    }

    /// <summary>
    /// Updates the grid logic from the game state payload.
    /// </summary>
    /// <param name="payload">The game state payload.</param>
    public void UpdateGridLogic(GameStatePayload payload)
    {
        components.Grid.Logic.UpdateFromGameStatePayload(payload);
    }

    /// <summary>
    /// Updates the list of players.
    /// </summary>
    /// <param name="updatedPlayers">
    /// The list of players received from the server.
    /// </param>
    public void UpdatePlayers(List<Player> updatedPlayers)
    {
        foreach (Player updatedPlayer in updatedPlayers)
        {
            if (players.TryGetValue(updatedPlayer.Id, out var existingPlayer))
            {
                existingPlayer.UpdateFrom(updatedPlayer);
            }
            else
            {
                players[updatedPlayer.Id] = updatedPlayer;
            }
        }

        players
            .Where(x => !updatedPlayers.Contains(x.Value))
            .ToList()
            .ForEach(x => players.Remove(x.Key));
    }

    /// <summary>
    /// Refreshes the player bar panels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method adjusts the player bar panels
    /// to the current list of players.
    /// </para>
    /// <para>
    /// Should be called after <see cref="UpdatePlayers"/>.
    /// </para>
    /// </remarks>
    public void RefreshPlayerBarPanels()
    {
        components.PlayerIdentityBarPanel.Refresh(players);
        components.PlayerStatsBarPanel.Refresh(players);
    }

    /// <summary>
    /// Updates the player's fog of war.
    /// </summary>
    /// <param name="payload">The player's game state payload.</param>
    public void UpdatePlayerFogOfWar(GameStatePayload.ForPlayer payload)
    {
        var player = players[payload.PlayerId];
        components.Grid.UpdateFogOfWar(payload.VisibilityGrid, new Color(player.Color));
    }

    /// <summary>
    /// Updates the timer.
    /// </summary>
    /// <param name="tick">The current tick of the game.</param>
    public void UpdateTimer(int tick)
    {
        var time = Game.ServerBroadcastInterval * tick;
        components.Timer.Time = time;
    }
}
