using GameClient.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the game scene.
/// </summary>
internal class Game : Scene
{
    private Tank tank = default!;
    private Text pressedKeys = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game()
        : base(Color.DimGray)
    {
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        string text = "Pressed keys:  ";

        foreach (Keys key in KeyboardController.GetPressedKeys())
        {
            text += key.ToString() + ",  ";
        }

        this.pressedKeys.Value = text;
        this.tank.Update(gameTime);

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        this.tank.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        var font = new ScalableFont("Content\\Fonts\\Consolas.ttf", 18);

        this.pressedKeys = new Text(font, Color.White)
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.TopLeft,
                RelativeOffset = new Vector2(0.04f, 0.04f),
            },
        };

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
        backBtn.Clicked += (s, e) => ChangeToPreviousOr<MainMenu>();
        backBtn.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.MainMenu");

        var settingsBtn = new Button<Frame>(new Frame())
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.18f, -0.04f),
                RelativeSize = new Vector2(0.12f, 0.07f),
            },
        }.ApplyStyle(Styles.UI.ButtonStyle);
        settingsBtn.Clicked += (s, e) => ShowOverlay<Settings>(new OverlayShowOptions(BlockFocusOnUnderlyingScenes: true));
        settingsBtn.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Settings");

        this.tank = new Tank(Color.Red);
    }
}
