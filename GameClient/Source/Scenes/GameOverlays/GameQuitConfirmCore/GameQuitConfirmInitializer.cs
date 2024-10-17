using System;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes.GameOverlays.GameQuitConfirmCore;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Represents a game quit confirm scene initializer.
/// </summary>
/// <param name="gameQuitConfirm">The game quit confirm scene.</param>
internal class GameQuitConfirmInitializer(GameQuitConfirm gameQuitConfirm)
{
    /// <summary>
    /// Creates a title.
    /// </summary>
    /// <returns>The created title.</returns>
    public LocalizedWrappedText CreateQuestion()
    {
        var titleFont = new ScalableFont("Content\\Fonts\\Orbitron-SemiBold.ttf", (int)(33 * ScreenController.Scale.Y));

        var text = new LocalizedWrappedText(titleFont, Color.White)
        {
            Parent = gameQuitConfirm.BaseComponent,
            Value = new LocalizedString("ConfirmQuestion.LeaveMatch"),
            Spacing = 20,
            Case = TextCase.Upper,
            TextAlignment = Alignment.BottomLeft,
            Transform =
            {
                RelativeSize = new Vector2(0.7f, 0.5f),
                Alignment = Alignment.TopLeft,
                RelativeOffset = new Vector2(0.06f, 0.3f),
            },
        };

        ScreenController.ScreenChanged += (s, e) =>
        {
            text.Font = new ScalableFont(
                "Content\\Fonts\\Orbitron-SemiBold.ttf",
                (int)(33 * ScreenController.Scale.Y));
            text.ForceUpdate(withTransform: true);
        };

        return text;
    }

    /// <summary>
    /// Creates a button list box.
    /// </summary>
    /// <returns>The created button list box.</returns>
    public ListBox CreateButtonListBox()
    {
        return new ListBox
        {
            Parent = gameQuitConfirm.BaseComponent,
            IsPriority = true,
            Transform =
            {
                Alignment = Alignment.Left,
                RelativeOffset = new Vector2(0.06f, 0.2f),
                RelativeSize = new Vector2(0.4f, 0.4f),
            },
        };
    }

    /// <summary>
    /// Creates a button.
    /// </summary>
    /// <param name="listBox">The list box to which the button will be added.</param>
    /// <param name="text">The localized string of the button.</param>
    /// <param name="iconPath">The path to the icon of the button.</param>
    /// <param name="action">The action to be performed when the button is clicked.</param>
    /// <returns>The created button.</returns>
    public Button<Container> CreateButton(ListBox listBox, LocalizedString text, string iconPath, Action action)
    {
        var button = new Button<Container>(new Container())
        {
            Parent = listBox.ContentContainer,
            Transform =
            {
                RelativeSize = new Vector2(1, 0.20f),
            },
        };

        var style = Styles.UI.GetButtonStyleWithIcon(text, iconPath, Alignment.Left);
        button.ApplyStyle(style);

        button.Clicked += (s, e) => action();

        return button;
    }
}
