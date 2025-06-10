using GameClient.Networking;
using MonoRivUI;

namespace GameClient.Scenes.GameOverlays.GameQuitConfirmCore;

/// <summary>
/// Represents the components of the game quit confirm scene.
/// </summary>
internal class GameQuitConfirmComponents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameQuitConfirmComponents"/> class.
    /// </summary>
    /// <param name="initializer">The game quit confirm scene initializer.</param>
    public GameQuitConfirmComponents(GameQuitConfirmInitializer initializer)
    {
        this.Question = initializer.CreateQuestion();
        this.ButtonListBox = initializer.CreateButtonListBox();

        this.LeaveButton = initializer.CreateButton(
            this.ButtonListBox,
            new LocalizedString("Buttons.Leave"),
            "Images/Icons/exit.svg",
            async () =>
            {
                await ServerConnection.CloseAsync("Leave match");
#if STEREO
                GameClient.GameClientCore.Bots.ForEach(x => x.Stop());
                GameClient.GameClientCore.Server?.Stop();
#endif
                Scene.HideOverlay<GameQuitConfirm>();
                Scene.Change<MainMenu>();
            });

        this.StayButton = initializer.CreateButton(
            this.ButtonListBox,
            new LocalizedString("Buttons.Stay"),
            "Images/Icons/back.svg",
            () =>
            {
                var options = new OverlayShowOptions(BlockFocusOnUnderlyingScenes: true);
                Scene.HideOverlay<GameQuitConfirm>();
                Scene.ShowOverlay<GameMenu>(options);
            });
    }

    /// <summary>
    /// Gets the question text component.
    /// </summary>
    public LocalizedWrappedText Question { get; }

    /// <summary>
    /// Gets the list box of the buttons.
    /// </summary>
    public ListBox ButtonListBox { get; }

    /// <summary>
    /// Gets the leave button.
    /// </summary>
    public Button<Container> LeaveButton { get; }

    /// <summary>
    /// Gets the stay button.
    /// </summary>
    public Button<Container> StayButton { get; }
}
