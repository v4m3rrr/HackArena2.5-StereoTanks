using System.Collections.Generic;
using System.Linq;
using GameLogic;

namespace GameClient.Scenes.GameEndCore;

/// <summary>
/// Represents a game end updater.
/// </summary>
/// <param name="components">The game end screen components.</param>
internal class GameEndUpdater(GameEndComponents components)
{
    /// <summary>
    /// Updates the scoreboard.
    /// </summary>
    /// <param name="players">
    /// The sorted players to update the scoreboard with.
    /// </param>
    public void UpdateScoreboard(IEnumerable<Player> players)
    {
        components.Scoreboard.SetPlayers(players.ToArray());
    }
}
