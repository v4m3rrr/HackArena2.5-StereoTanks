using GameLogic;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

/// <summary>
/// Represents a player bar.
/// </summary>
/// <param name="player">The player the bar represents.</param>
internal abstract class PlayerBar(Player player) : BaseBar
{
    /// <summary>
    /// Gets the player the bar represents.
    /// </summary>
    public Player Player { get; } = player;
}
