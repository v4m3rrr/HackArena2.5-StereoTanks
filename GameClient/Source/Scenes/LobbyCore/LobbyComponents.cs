using System.Collections.Generic;
using GameClient.LobbySceneComponents;
using MonoRivUI;

namespace GameClient.Scenes.LobbyCore;

/// <summary>
/// Represents the lobby components.
/// </summary>
internal class LobbyComponents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LobbyComponents"/> class.
    /// </summary>
    /// <param name="initializer">
    /// The lobby initializer that will be used to create the lobby components.
    /// </param>
    public LobbyComponents(LobbyInitializer initializer)
    {
        this.MatchName = initializer.CreateMatchName();
        this.JoinCode = initializer.CreateJoinCode();
        this.PlayerSlotPanels = initializer.CreatePlayerSlotPanels();
        this.LeaveButton = initializer.CreateLeaveButton();
    }

    /// <summary>
    /// Gets the match name text component.
    /// </summary>
    public Text MatchName { get; }

    /// <summary>
    /// Gets the join code container component.
    /// </summary>
    public Text JoinCode { get; }

    /// <summary>
    /// Gets the player info components.
    /// </summary>
    public List<PlayerSlotPanel> PlayerSlotPanels { get; }

    /// <summary>
    /// Gets the leave button component.
    /// </summary>
    public Button<Container> LeaveButton { get; }
}
