using MonoRivUI;

namespace GameClient.Scenes.GameOverlays.GameMenuCore;

/// <summary>
/// Represents the components of the game menu.
/// </summary>
internal class GameMenuComponents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameMenuComponents"/> class.
    /// </summary>
    /// <param name="initializer">The game menu initializer.</param>
    public GameMenuComponents(GameMenuInitializer initializer)
    {
        this.Title = initializer.CreateTitle();
        this.ButtonListBox = initializer.CreateButtonListBox();

        this.SettingsButton = initializer.CreateButton(
            this.ButtonListBox,
            new LocalizedString("Buttons.Settings"),
            "Images/Icons/settings.svg",
            () =>
            {
                var options = new OverlayShowOptions(BlockFocusOnUnderlyingScenes: true);
                Scene.HideOverlay<GameMenu>();
                Scene.ShowOverlay<Settings>(options);
            });

        this.LeaveMatchButton = initializer.CreateButton(
            this.ButtonListBox,
            new LocalizedString("Buttons.LeaveMatch"),
            "Images/Icons/leave.svg",
            () =>
            {
                var options = new OverlayShowOptions(BlockFocusOnUnderlyingScenes: true);
                Scene.HideOverlay<GameMenu>();
                Scene.ShowOverlay<GameQuitConfirm>(options);
            });

        this.BackButton = initializer.CreateBackButton();
    }

    /// <summary>
    /// Gets the title of the game menu.
    /// </summary>
    public Text Title { get; }

    /// <summary>
    /// Gets the list box of the buttons.
    /// </summary>
    public ListBox ButtonListBox { get; }

    /// <summary>
    /// Gets the settings button.
    /// </summary>
    public Button<Container> SettingsButton { get; }

    /// <summary>
    /// Gets the leave match button.
    /// </summary>
    public Button<Container> LeaveMatchButton { get; }

    /// <summary>
    /// Gets the back button.
    /// </summary>
    public Button<Container> BackButton { get; }
}
