using System;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes.GameOverlays.GameMenuCore;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
/// Represents a game menu initializer.
/// </summary>
/// <param name="gameMenu">The game menu.</param>
internal class GameMenuInitializer(GameMenu gameMenu)
{
    /// <summary>
    /// Creates a title.
    /// </summary>
    /// <returns>The created title.</returns>
    public Text CreateTitle()
    {
        var titleFont = new ScalableFont("Content\\Fonts\\Orbitron-SemiBold.ttf", 45);
        return new Text(titleFont, Color.White)
        {
            Parent = gameMenu.BaseComponent,
            Value = "MONO TANKS",
            TextShrink = TextShrinkMode.HeightAndWidth,
            Spacing = 20,
            Transform =
            {
                Alignment = Alignment.TopLeft,
                RelativeOffset = new Vector2(0.06f, 0.33f),
            },
        };
    }

    /// <summary>
    /// Creates a button list box.
    /// </summary>
    /// <returns>The created button list box.</returns>
    public ListBox CreateButtonListBox()
    {
        return new ListBox
        {
            Parent = gameMenu.BaseComponent,
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

    /// <summary>
    /// Creates a back button.
    /// </summary>
    /// <returns>The created back button.</returns>
    public Button<Container> CreateBackButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = gameMenu.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.06f, -0.08f),
                RelativeSize = new Vector2(0.2f, 0.07f),
            },
        };

        var text = new LocalizedString("Buttons.Back");
        var iconPath = "Images/Icons/back.svg";
        var style = Styles.UI.GetButtonStyleWithIcon(text, iconPath, Alignment.Left);
        button.ApplyStyle(style);

        button.Clicked += (s, e) => Scene.HideOverlay<GameMenu>();

        return button;
    }
}
