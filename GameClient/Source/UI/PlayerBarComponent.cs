using GameLogic;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents a player bar component.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PlayerBarComponent"/> class.
/// </remarks>
/// <param name="player">The player the component is associated with.</param>
internal abstract class PlayerBarComponent(Player player) : Component
{
    /// <summary>
    /// Gets the player the component is associated with.
    /// </summary>
    public Player Player { get; private set; } = player;
}
