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
            TextShrink = TextShrinkMode.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.TopRight,
                RelativeOffset = new Vector2(-0.06f, 0.06f),
                RelativeSize = new Vector2(0.6f, 0.2f),
                MinSize = new Point(450, int.MaxValue),
                MaxSize = new Point(900, int.MaxValue),
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
                MinSize = new Point(500, 20),
                MaxSize = new Point(900, 100),
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
                MinSize = new Point(500, 20),
                MaxSize = new Point(900, 100),
            },
        };

        _ = CreateButton(listBoxTop, new LocalizedString("Buttons.CreateGame"), (s, e) => throw new NotImplementedException());
        _ = CreateButton(listBoxBottom, new LocalizedString("Buttons.JoinGame"), (s, e) => throw new NotImplementedException());
        _ = CreateButton(listBoxTop, new LocalizedString("Buttons.HowToPlay"), (s, e) => Change<HowToPlay>());
        _ = CreateButton(listBoxBottom, new LocalizedString("Buttons.Settings"), (s, e) => Change<Settings>());
        _ = CreateButton(listBoxTop, new LocalizedString("Buttons.Authors"), (s, e) => Change<Authors>());
        _ = CreateButton(listBoxBottom, new LocalizedString("Buttons.Exit"), (s, e) => MonoTanks.Instance.Exit());

#if DEBUG
        var quickJoinPlayerBtn = new Button<SolidColor>(new SolidColor(Color.DarkRed))
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.12f, -0.04f),
                RelativeSize = new Vector2(0.15f, 0.06f),
            },
        };
        quickJoinPlayerBtn.HoverEntered += (s, e) => e.Color = Color.Red;
        quickJoinPlayerBtn.HoverExited += (s, e) => e.Color = Color.DarkRed;
        quickJoinPlayerBtn.Clicked += (s, e) =>
        {
            var args = new Game.ChangeEventArgs(joinCode: null, isSpectator: false);
            Change<Game>(args);
        };
        _ = new Text(Styles.UI.ButtonStyle.GetPropertyOfType<ScalableFont>()!, Color.White)
        {
            Parent = quickJoinPlayerBtn.Component,
            Value = "Quick Join",
            Scale = 0.7f,
            TextAlignment = Alignment.Center,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Top,
            },
        };
        _ = new Text(Styles.UI.ButtonStyle.GetPropertyOfType<ScalableFont>()!, Color.White)
        {
            Parent = quickJoinPlayerBtn.Component,
            Value = "(player)",
            Scale = 0.7f,
            TextAlignment = Alignment.Center,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Bottom,
            },
        };

        var quickJoinSpectatorBtn = new Button<SolidColor>(new SolidColor(Color.DarkRed))
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.12f, -0.12f),
                RelativeSize = new Vector2(0.15f, 0.06f),
            },
        };
        quickJoinSpectatorBtn.HoverEntered += (s, e) => e.Color = Color.Red;
        quickJoinSpectatorBtn.HoverExited += (s, e) => e.Color = Color.DarkRed;
        quickJoinSpectatorBtn.Clicked += (s, e) =>
        {
            var args = new Game.ChangeEventArgs(joinCode: null, isSpectator: true);
            Change<Game>(args);
        };
        _ = new Text(Styles.UI.ButtonStyle.GetPropertyOfType<ScalableFont>()!, Color.White)
        {
            Parent = quickJoinSpectatorBtn.Component,
            Value = "Quick Join",
            Scale = 0.7f,
            TextAlignment = Alignment.Center,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Top,
            },
        };
        _ = new Text(Styles.UI.ButtonStyle.GetPropertyOfType<ScalableFont>()!, Color.White)
        {
            Parent = quickJoinSpectatorBtn.Component,
            Value = "(spectator)",
            Scale = 0.7f,
            TextAlignment = Alignment.Center,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Bottom,
            },
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
