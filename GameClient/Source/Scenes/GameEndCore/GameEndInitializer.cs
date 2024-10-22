using GameClient.UI.GameEndSceneComponents;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes.GameEndCore;

/// <summary>
/// Represents the initializer for the game end scene.
/// </summary>
/// <param name="gameEnd">The game end scene to initialize.</param>
internal class GameEndInitializer(GameEnd gameEnd)
{
#if HACKATHON

    /// <summary>
    /// Creates a match name component.
    /// </summary>
    /// <returns>The created match name component.</returns>
    public Text CreateMatchName()
    {
        var font = new ScalableFont(Styles.Fonts.Paths.Main, 21);
        return new Text(font, Color.White)
        {
            Parent = gameEnd.BaseComponent,
            Value = "Match name",
            Case = TextCase.Upper,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Spacing = 10,
            Transform =
            {
                Alignment = Alignment.Top,
                RelativeOffset = new Vector2(0.0f, 0.1f),
            },
        };
    }

#endif

    /// <summary>
    /// Creates a continue button.
    /// </summary>
    /// <returns>The created continue button.</returns>
    public Button<Container> CreateContinueButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = gameEnd.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.08f, -0.08f),
                RelativeSize = new Vector2(0.2f, 0.07f),
            },
        };

        var text = new LocalizedString("Buttons.Continue");
        var iconPath = "Images/Icons/continue.svg";
        var style = Styles.UI.GetButtonStyleWithIcon(text, iconPath, Alignment.Right);

        button.ApplyStyle(style);

        button.Clicked += (s, e) => Scene.Change<MainMenu>();

        return button;
    }

    /// <summary>
    /// Initializes the scoreboard component.
    /// </summary>
    /// <returns>The initialized scoreboard component.</returns>
    public Scoreboard InitializeScoreboard()
    {
        return new Scoreboard()
        {
            Parent = gameEnd.BaseComponent,
            Transform =
            {
                Alignment = Alignment.Center,
                RelativeSize = new Vector2(0.6f, 0.6f),
                MinSize = new Point(620, 1),
            },
        };
    }
}
