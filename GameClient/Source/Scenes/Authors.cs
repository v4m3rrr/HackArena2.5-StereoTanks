using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the authors scene.
/// </summary>
/// <remarks>
/// This scene displays the authors of the game.
/// </remarks>
internal class Authors : Scene
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Authors"/> class.
    /// </summary>
    public Authors()
        : base(Color.OliveDrab * 0.7f)
    {
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        var font = new ScalableFont("Content\\Fonts\\Tiny5-Regular.ttf", 26);

        var backBtn = new Button<Frame>(new Frame())
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.04f, -0.04f),
                RelativeSize = new Vector2(0.12f, 0.07f),
            },
        }.ApplyStyle(Styles.UI.ButtonStyle);
        backBtn.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Back");
        backBtn.Clicked += (s, e) => ChangeToPreviousOr<MainMenu>();

        var container = new Container()
        {
            Parent = baseComponent,
            Transform =
            {
                Alignment = Alignment.Top,
                RelativeOffset = new Vector2(0.0f, 0.1f),
                RelativeSize = new Vector2(0.235f, 0.6f),
            },
        };

        var frame = new Frame(Color.Red, 3)
        {
            Parent = container,
            Transform =
            {
                Alignment = Alignment.Top,
                RelativeSize = new Vector2(1.0f, 0.7f),
            },
        };

        var image = new Image("RivixProfilePicture")
        {
            Parent = frame.InnerContainer,
        };

        var text = new Text(font, Color.Red)
        {
            Parent = container,
            Value = "RIVIX",
            TextAlignment = Alignment.Top,
            Transform =
            {
                Alignment = Alignment.Bottom,
                RelativeSize = new Vector2(1.0f, 0.2f),
            },
        };
    }
}
