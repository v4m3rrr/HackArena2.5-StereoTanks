using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the main menu scene.
/// </summary>
public class MainMenu : Scene
{
#if DEBUG
    private Text openDebugConsoleInfo = default!;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="MainMenu"/> class.
    /// </summary>
    public MainMenu()
        : base(Color.Black)
    {
    }

#if DEBUG
    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        this.openDebugConsoleInfo.IsEnabled = !DisplayedOverlays.Any(x => x.Scene is DebugConsole);
        base.Update(gameTime);
    }
#endif

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        // Background image
        _ = new Image("monoTanksBGg") { Parent = this.BaseComponent, Transform = { Alignment = Alignment.Center } };

        // Title
        var titleFont = new ScalableFont("Content\\Fonts\\Tiny5-Regular.ttf", 76);
        _ = new Text(titleFont, Color.White)
        {
            Parent = this.BaseComponent,
            Value = "MONO TANKS",
            TextAlignment = Alignment.TopRight,
            Transform =
            {
                RelativeOffset = new(-0.06f, 0.06f),
            },
        };

        // Buttons
        var listBoxTop = new ListBox()
        {
            Parent = this.BaseComponent,
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            IsScrollable = false,
            ResizeContent = true,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.03f, -0.12f),
                RelativeSize = new Vector2(0.66f, 0.06f),
            },
        };

        var listBoxBottom = new ListBox()
        {
            Parent = this.BaseComponent,
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            IsScrollable = false,
            ResizeContent = true,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.03f, -0.05f),
                RelativeSize = new Vector2(0.66f, 0.06f),
            },
        };

        _ = CreateButton(listBoxTop, new LocalizedString("Buttons.CreateGame"), (s, e) => throw new NotImplementedException());
        _ = CreateButton(listBoxBottom, new LocalizedString("Buttons.JoinGame"), (s, e) => throw new NotImplementedException());
        _ = CreateButton(listBoxTop, new LocalizedString("Buttons.HowToPlay"), (s, e) => Change<HowToPlay>());
        _ = CreateButton(listBoxBottom, new LocalizedString("Buttons.Settings"), (s, e) => Change<Settings>());
        _ = CreateButton(listBoxTop, new LocalizedString("Buttons.Authors"), (s, e) => Change<Authors>());
        _ = CreateButton(listBoxBottom, new LocalizedString("Buttons.Exit"), (s, e) => GameClient.Instance.Exit());

#if DEBUG
        var quickStartBtn = new Button<SolidColor>(new SolidColor(Color.DarkRed))
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.12f, -0.04f),
                RelativeSize = new Vector2(0.15f, 0.06f),
            },
        };
        quickStartBtn.HoverEntered += (s, e) => e.Color = Color.Red;
        quickStartBtn.HoverExited += (s, e) => e.Color = Color.DarkRed;
        quickStartBtn.Clicked += (s, e) => Change<Game>();
        _ = new Text(Styles.UI.ButtonStyle.GetPropertyOfType<ScalableFont>(), Color.White)
        {
            Parent = quickStartBtn.Component,
            Value = "Quick Start",
            TextAlignment = Alignment.Center,
        };

        this.openDebugConsoleInfo = new Text(new ScalableFont("Content\\Fonts\\Consolas.ttf", 9), Color.LightGray)
        {
            Parent = this.BaseComponent,
            Value = "Press [CTRL + `] to open the debug console.",
            TextShrink = TextShrinkMode.HeightAndWidth,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.03f, -0.015f),
            },
        };
#endif
    }

    private static Button<Frame> CreateButton(ListBox listBox, LocalizedString text, EventHandler clicked)
    {
        var button = new Button<Frame>(new Frame()) { Parent = listBox.ContentContainer };
        _ = button.ApplyStyle(Styles.UI.ButtonStyle);
        button.GetDescendant<LocalizedText>()!.Value = text;
        button.Clicked += clicked;
        return button;
    }
}
