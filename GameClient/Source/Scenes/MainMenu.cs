using GameClient.Networking;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the main menu scene.
/// </summary>
internal class MainMenu : Scene
{
    private Text title = default!;
    private ScalableTexture2D logo = default!;
    private ScalableTexture2D element = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainMenu"/> class.
    /// </summary>
    public MainMenu()
        : base(Color.Black)
    {
    }

    /// <summary>
    /// Gets the background texture.
    /// </summary>
    public static ScalableTexture2D Effect { get; private set; } = default!;

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        Effect.Rotation += 0.1f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Effect.Rotation %= MathHelper.TwoPi;

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        Effect = new ScalableTexture2D("Images/MainMenu/effect.svg")
        {
            Parent = baseComponent,
            Color = MonoTanks.ThemeColor,
            Transform =
            {
                RelativeSize = new Vector2(1.924f, 2.206f),
                Alignment = Alignment.Center,
            },
        };

        Effect.Load();
        Effect.RelativeOrigin = new Vector2(0.5f);
        Effect.Transform.SetRelativeOffsetFromAbsolute(Effect.Texture.Bounds.Center);

        this.element = new ScalableTexture2D("Images/MainMenu/background_element.svg")
        {
            Parent = baseComponent,
            Transform =
            {
                RelativeSize = new Vector2(0.45f),
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.08f, -0.14f),
            },
        };

        this.element.Load();

        var titleFont = new ScalableFont("Content\\Fonts\\Orbitron-SemiBold.ttf", 42);
        this.title = new Text(titleFont, Color.White)
        {
            Parent = baseComponent,
            Value = "MONO TANKS",
#if DEBUG
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
#endif
            TextShrink = TextShrinkMode.HeightAndWidth,
            Spacing = 20,
            Transform =
            {
                Alignment = Alignment.TopLeft,
                RelativeOffset = new Vector2(0.06f, 0.28f),
            },
        };

#if DEBUG
        var debugInfoFont = new ScalableFont("Content\\Fonts\\Orbitron-SemiBold.ttf", 14);
        _ = new Text(debugInfoFont, Color.White * 0.9f)
        {
            Parent = this.title,
            Value = "[Debug]",
            TextAlignment = Alignment.Right,
            Spacing = 52,
            Transform =
            {
                RelativeSize = new Vector2(0.1f),
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(0.0f, 0.5f),
            },
        };
#endif

        this.logo = new ScalableTexture2D("Images/logo.svg")
        {
            Parent = baseComponent,
            Transform =
            {
                RelativeSize = new Vector2(0.1f),
                Alignment = Alignment.TopRight,
                RelativeOffset = new Vector2(-0.08f, 0.08f),
            },
        };

        this.logo.Load();

        var listBox = new ListBox
        {
            Parent = baseComponent,
            Spacing = 15,
            Transform =
            {
                RelativeSize = new Vector2(0.32f, 0.32f),
                Alignment = Alignment.Left,
                RelativeOffset = new Vector2(0.06f, 0.08f),
            },
        };

        var joinRoomBtn = CreateButton(new LocalizedString("Buttons.JoinGame"), listBox, "join_room_icon");
        joinRoomBtn.Clicked += (s, e) => Change<JoinRoom>();

        var settingsBtn = CreateButton(new LocalizedString("Buttons.Settings"), listBox, "settings_icon");
        settingsBtn.Clicked += (s, e) => Change<Settings>();

        var authorsBtn = CreateButton(new LocalizedString("Buttons.Authors"), listBox, "authors_icon");
        authorsBtn.Clicked += (s, e) => Change<Authors>();

        var exitBtn = CreateButton(new LocalizedString("Buttons.Exit"), listBox, "exit_icon");
        exitBtn.Clicked += (s, e) => MonoTanks.Instance.Exit();

#if DEBUG
        var quickJoinFont = new ScalableFont("Content\\Fonts\\Orbitron-SemiBold.ttf", 9);

        async void Connect<T>(bool isSpectator)
            where T : Scene
        {
            ServerConnection.ErrorThrew += DebugConsole.ThrowError;

            string? joinCode = null;
            ConnectionData connectionData;

            if (isSpectator)
            {
                connectionData = ConnectionData.ForSpectator("localhost:5000", joinCode);
            }
            else
            {
                connectionData = ConnectionData.ForPlayer("localhost:5000", joinCode, "player", true);
            }

            if (await ServerConnection.ConnectAsync(connectionData))
            {
                var args = new GameDisplayEventArgs(joinCode, isSpectator);
                Change<T>(args);
            }

            ServerConnection.ErrorThrew -= DebugConsole.ThrowError;
        }

        // Lobby
        {
            var quickJoinPlayerBtn = new Button<SolidColor>(new SolidColor(Color.DarkRed))
            {
                Parent = this.BaseComponent,
                Transform =
            {
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.3f, -0.04f),
                RelativeSize = new Vector2(0.15f, 0.06f),
            },
            };
            quickJoinPlayerBtn.HoverEntered += (s, e) => e.Color = Color.Red;
            quickJoinPlayerBtn.HoverExited += (s, e) => e.Color = Color.DarkRed;
            quickJoinPlayerBtn.Clicked += (s, e) => Connect<Lobby>(false);
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinPlayerBtn.Component,
                Value = "Quick Lobby",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
                Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Top,
            },
            };
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinPlayerBtn.Component,
                Value = "(player)",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
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
                RelativeOffset = new Vector2(-0.3f, -0.12f),
                RelativeSize = new Vector2(0.15f, 0.06f),
            },
            };
            quickJoinSpectatorBtn.HoverEntered += (s, e) => e.Color = Color.Red;
            quickJoinSpectatorBtn.HoverExited += (s, e) => e.Color = Color.DarkRed;
            quickJoinSpectatorBtn.Clicked += (s, e) => Connect<Lobby>(true);
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinSpectatorBtn.Component,
                Value = "Quick Lobby",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
                Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Top,
            },
            };
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinSpectatorBtn.Component,
                Value = "(spectator)",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
                Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Bottom,
            },
            };
        }

        // Game
        {
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
            quickJoinPlayerBtn.Clicked += (s, e) => Connect<Game>(false);
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinPlayerBtn.Component,
                Value = "Quick Join",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
                Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Top,
            },
            };
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinPlayerBtn.Component,
                Value = "(player)",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
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
            quickJoinSpectatorBtn.Clicked += (s, e) => Connect<Game>(true);
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinSpectatorBtn.Component,
                Value = "Quick Join",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
                Transform =
                {
                    RelativeSize = new Vector2(1f, 0.5f),
                    Alignment = Alignment.Top,
                },
            };
            _ = new Text(quickJoinFont, Color.White)
            {
                Parent = quickJoinSpectatorBtn.Component,
                Value = "(spectator)",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.Width,
                Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Bottom,
            },
            };
        }
#endif
    }

    private static IButton<Container> CreateButton(
        LocalizedString text,
        ListBox listbox,
        string iconName)
    {
        var button = new Button<Container>(new Container())
        {
            Parent = listbox.ContentContainer,
            Transform =
            {
                RelativeSize = new Vector2(1, 0.20f),
            },
        };

        var iconPath = $"Images/MainMenu/{iconName}.svg";
        var style = Styles.UI.GetButtonStyleWithIcon(text, iconPath, Alignment.Left);
        button.ApplyStyle(style);

        return button;
    }
}
