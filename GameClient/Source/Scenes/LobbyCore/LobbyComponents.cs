using System.Collections.Generic;
using GameClient.UI.LobbySceneComponents;
using MonoRivUI;

namespace GameClient.Scenes.LobbyCore;

/// <summary>
/// Represents the lobby components.
/// </summary>
/// <param name="initializer">
/// The lobby initializer that will be used to create the lobby components.
/// </param>
internal class LobbyComponents(LobbyInitializer initializer)
{
#if HACKATHON

    /// <summary>
    /// Gets the match name text component.
    /// </summary>
    public Text MatchName { get; } = initializer.CreateMatchName();

#endif

    /// <summary>
    /// Gets the join code container component.
    /// </summary>
    public Text JoinCode { get; } = initializer.CreateJoinCode();

#if STEREO

    /// <summary>
    /// Gets the team slot panels.
    /// </summary>
    public List<TeamSlotPanel> TeamSlotPanels { get; } = initializer.CreateTeamSlotPanels();

#else

    /// <summary>
    /// Gets the player slot panels.
    /// </summary>
    public List<PlayerSlotPanel> PlayerSlotPanels { get; } = initializer.CreatePlayerSlotPanels();

#endif

    /// <summary>
    /// Gets the leave button component.
    /// </summary>
    public Button<Container> LeaveButton { get; } = initializer.CreateLeaveButton();

    /// <summary>
    /// Gets the continue button component.
    /// </summary>
    /// <remarks>
    /// Currently used to start the replay.
    /// </remarks>
    public Button<Container> ContinueButton { get; } = initializer.CreateContinueButton();
}
