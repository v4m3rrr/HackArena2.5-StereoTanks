using GameClient.GameSceneComponents.PlayerBarComponents;
using GameLogic;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

/// <summary>
/// Represents a player bar component.
/// </summary>
/// <param name="player">The player the component is associated with.</param>
internal abstract class PlayerBarComponent(Player player) : Component
{
    /// <summary>
    /// Gets the player the component is associated with.
    /// </summary>
    public Player Player { get; private set; } = player;

    /// <summary>
    /// Loads the content of the component.
    /// </summary>
    public static void LoadContent()
    {
        BulletCount.LoadStaticContent();
#if STEREO
        Abilities.LoadStaticContent();
#else
        SecondaryItems.LoadStaticContent();
#endif
    }
}
