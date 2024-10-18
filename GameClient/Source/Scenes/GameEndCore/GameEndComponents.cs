using GameClient.UI.GameEndSceneComponents;
using MonoRivUI;

namespace GameClient.Scenes.GameEndCore;

/// <summary>
/// Represents the game end components.
/// </summary>
/// <param name="initializer">
/// The game end initializer that will be used to create the game end components.
/// </param>
internal class GameEndComponents(GameEndInitializer initializer)
{
#if HACKATHON

    /// <summary>
    /// Gets the match name text component.
    /// </summary>
    public Text MatchName { get; } = initializer.CreateMatchName();

#endif

    /// <summary>
    /// Gets the continue button component.
    /// </summary>
    public Button<Container> ContinueButton { get; } = initializer.CreateContinueButton();

    /// <summary>
    /// Gets the scoreboard component.
    /// </summary>
    public Scoreboard Scoreboard { get; } = initializer.InitializeScoreboard();
}
