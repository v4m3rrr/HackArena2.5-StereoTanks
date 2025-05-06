using System;
using GameClient.GameSceneComponents;
using MonoRivUI;

namespace GameClient.Scenes.GameCore;

/// <summary>
/// Represents the game components.
/// </summary>
internal class GameComponents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameComponents"/> class.
    /// </summary>
    /// <param name="initializer">
    /// The game initializer that will be used to create the game components.
    /// </param>
    public GameComponents(GameInitializer initializer)
    {
#if STEREO
        this.TeamBarPanels = initializer.CreateTeamBarPanels();
#else
        var (identityPanel, statsPanel) = initializer.CreatePlayerBarPanels();
        this.PlayerIdentityBarPanel = identityPanel;
        this.PlayerStatsBarPanel = statsPanel;
#endif

        this.Grid = initializer.CreateGridComponent();
        this.Timer = initializer.CreateTimer();
        this.MenuButton = initializer.CreateMenuButton();

#if HACKATHON
        this.MatchName = initializer.CreateMatchName();
#endif
    }

#if STEREO

    /// <summary>
    /// Gets the team bar panel.
    /// </summary>
    public IEnumerable<TeamBarPanel> TeamBarPanels { get; }

#else

    /// <summary>
    /// Gets the player identity bar panel.
    /// </summary>
    public PlayerBarPanel<PlayerIdentityBar> PlayerIdentityBarPanel { get; }

    /// <summary>
    /// Gets the player stats bar panel.
    /// </summary>
    public PlayerBarPanel<PlayerStatsBar> PlayerStatsBarPanel { get; }

#endif

    /// <summary>
    /// Gets the grid component.
    /// </summary>
    public GridComponent Grid { get; }

    /// <summary>
    /// Gets the timer component.
    /// </summary>
    public Timer Timer { get; }

#if HACKATHON

    /// <summary>
    /// Gets the match name component.
    /// </summary>
    public Text MatchName { get; }

#endif

    /// <summary>
    /// Gets the menu button.
    /// </summary>
    public Button<Container> MenuButton { get; }
}
