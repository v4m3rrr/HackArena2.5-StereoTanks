using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameClient.Networking;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the main menu scene.
/// </summary>
[AutoInitialize]
[AutoLoadContent]
internal class MainMenu : Scene
{
    private Text title = default!;
    private ScalableTexture2D logo = default!;
    private ScalableTexture2D element = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainMenu"/> class.
    /// </summary>
    public MainMenu()
        : base(Color.Transparent)
    {
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        MainEffect.Rotation += 0.05f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        MainEffect.Rotation %= MathHelper.TwoPi;

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        MainEffect.Draw();
        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
#if STEREO
        this.element = new ScalableTexture2D("Images/MainMenu/background_element_stereo.svg")
#else
        this.element = new ScalableTexture2D("Images/MainMenu/background_element.svg")
#endif
        {
            Parent = baseComponent,
            Transform =
            {
                RelativeSize = new Vector2(0.45f),
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.08f, -0.14f),
            },
        };

        var titleFont = new ScalableFont(Styles.Fonts.Paths.Main, 53)
        {
            AutoResize = true,
            Spacing = 20,
        };

        this.title = new Text(titleFont, Color.White)
        {
            Parent = baseComponent,
#if STEREO
            Value = "STEREO TANKS",
#else
            Value = "MONO TANKS",
#endif
#if DEBUG
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
#endif
            TextShrink = TextShrinkMode.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.TopLeft,
                RelativeOffset = new Vector2(0.06f, 0.28f),
            },
        };

#if DEBUG
        var debugInfoFont = new ScalableFont(Styles.Fonts.Paths.Main, 24)
        {
            AutoResize = true,
            Spacing = 52,
        };

        _ = new Text(debugInfoFont, Color.White * 0.9f)
        {
            Parent = this.title,
            Value = "[Debug]",
            TextAlignment = Alignment.Right,
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

        var listBox = new ListBox
        {
            Parent = baseComponent,
            Transform =
            {
                RelativeSize = new Vector2(0.4f, 0.42f),
                Alignment = Alignment.Left,
                RelativeOffset = new Vector2(0.06f, 0.18f),
            },
        };
#if STEREO
        var singlePlayerBtn = CreateButton(new LocalizedString("Buttons.SinglePlayer"), listBox, "single");
        singlePlayerBtn.Clicked += (s, e) => Change<SinglePlayer>();

        var coopBtn = CreateButton(new LocalizedString("Buttons.SinglePlayer"), listBox, "single");
        coopBtn.Clicked += (s, e) => Change<Coop>();
#endif
        var joinRoomBtn = CreateButton(new LocalizedString("Buttons.JoinGame"), listBox, "multi");
        joinRoomBtn.Clicked += (s, e) => Change<JoinRoom>();

        var watchReplayBtn = CreateButton(new LocalizedString("Buttons.WatchReplay"), listBox, "watch_replay");
        watchReplayBtn.Clicked += (s, e) => Change<Replay.ChooseReplay>();

        var settingsBtn = CreateButton(new LocalizedString("Buttons.Settings"), listBox, "settings");
        settingsBtn.Clicked += (s, e) => Change<Settings>();

        var exitBtn = CreateButton(new LocalizedString("Buttons.Exit"), listBox, "exit");
        exitBtn.Clicked += (s, e) => GameClientCore.Instance.Exit();

#if DEBUG

        var quickJoinListBox = new FlexListBox
        {
            Parent = baseComponent,
            Spacing = 10,
            Transform =
            {
#if STEREO
                RelativeSize = new Vector2(0.18f, 0.36f),
#else
                RelativeSize = new Vector2(0.18f, 0.16f),
#endif
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.08f, -0.06f),
            },
        };

        var quickJoinFont = new ScalableFont(Styles.Fonts.Paths.Main, 9);

#if STEREO
        _ = CreateQuickJoinPlayerButton(quickJoinListBox, quickJoinFont, TankType.Light, "Team1");
        _ = CreateQuickJoinPlayerButton(quickJoinListBox, quickJoinFont, TankType.Heavy, "Team1");
        _ = CreateQuickJoinPlayerButton(quickJoinListBox, quickJoinFont, TankType.Light, "Team2");
        _ = CreateQuickJoinPlayerButton(quickJoinListBox, quickJoinFont, TankType.Heavy, "Team2");
#else
        _ = CreateQuickJoinPlayerButton(quickJoinListBox, quickJoinFont);
#endif

        // Quick join to game as a spectator
        {
            var quickJoinSpectatorBtn = new Button<SolidColor>(new SolidColor(Color.DarkRed))
            {
                Parent = quickJoinListBox.ContentContainer,
            };
            quickJoinSpectatorBtn.HoverEntered += (s, e) => e.Color = Color.Red;
            quickJoinSpectatorBtn.HoverExited += (s, e) => e.Color = Color.DarkRed;
            quickJoinSpectatorBtn.Clicked += (s, e) => QuickConnectSpectator();
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

        var versionFont = new ScalableFont(Styles.Fonts.Paths.Main, 6)
        {
            AutoResize = true,
            Spacing = 3,
            MinSize = 5,
        };

        var assembly = typeof(GameClientCore).Assembly;
        var version = assembly.GetName().Version!;

        var configuration = assembly
            .GetCustomAttribute<AssemblyConfigurationAttribute>()!
            .Configuration;

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var sb = new StringBuilder();
        sb.Append('v');

        if (informationalVersion is not null)
        {
#if RELEASE
            sb.Append(informationalVersion.Split('+')[0]);
#else
            sb.Append(informationalVersion);
#endif
        }
        else
        {
            sb.Append(version.Major)
                .Append('.')
                .Append(version.Minor)
                .Append('.')
                .Append(version.Build);
        }

#if RELEASE
        string versionText = sb.ToString();
#endif

        sb.Append(" (")
            .Append(GameClientCore.Platform)
            .Append(')');

#if DEBUG
        string versionText = sb.ToString();
#endif

        _ = new Text(versionFont, Color.White * 0.8f)
        {
            Parent = this.BaseComponent,
            Value = versionText,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            TextAlignment = Alignment.BottomLeft,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.012f, -0.02f),
            },
        };

        sb.Append(" [")
            .Append(configuration)
            .Append(']');

        DebugConsole.SendMessage("Version: " + sb.ToString(), new Color(0xC5, 0x48, 0xFF));
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        var textures = this.BaseComponent.GetAllDescendants<TextureComponent>();
        textures.ToList().ForEach(x => x.Load());
    }

#if DEBUG

#if STEREO
    private static async void QuickConnectPlayer(TankType tankType, string teamName)
#else
    private static async void QuickConnectPlayer()
#endif
    {
        var data = new ConnectionPlayerData("localhost:5000", joinCode: null)
        {
#if STEREO
            TeamName = teamName,
            TankType = tankType,
#else
            Nickname = "player",
#endif
            QuickJoin = true,
        };

        await QuickConnect<Game>(data);
    }

    private static async void QuickConnectSpectator()
    {
        var data = new ConnectionSpectatorData("localhost:5000", joinCode: null)
        {
            QuickJoin = true,
        };

        await QuickConnect<Game>(data);
    }

    private static async Task QuickConnect<T>(ConnectionData connectionData)
        where T : Scene
    {
        ServerConnection.ErrorThrew += DebugConsole.ThrowError;
        ConnectionStatus status = await ServerConnection.ConnectAsync(connectionData, CancellationToken.None);

        if (status is ConnectionStatus.Success)
        {
            Change<T>();
        }
        else if (status is ConnectionStatus.Failed failed && failed.Exception is not null)
        {
            DebugConsole.ThrowError("Connection failed!");
            DebugConsole.ThrowError(failed.Exception);
        }
        else if (status is ConnectionStatus.Rejected rejected)
        {
            DebugConsole.ThrowError($"Connection rejected: {rejected.Reason}");
        }
        else
        {
            DebugConsole.ThrowError("Failed to connect to the server.");
        }

        ServerConnection.ErrorThrew -= DebugConsole.ThrowError;
    }

#if STEREO
    private static Button<SolidColor> CreateQuickJoinPlayerButton(ListBox listBox, ScalableFont font, TankType tankType, string teamName)
#else
    private static Button<SolidColor> CreateQuickJoinPlayerButton(ListBox listBox, ScalableFont font)
#endif
    {
        var button = new Button<SolidColor>(new SolidColor(Color.DarkRed))
        {
            Parent = listBox.ContentContainer,
        };
        button.HoverEntered += (s, e) => e.Color = Color.Red;
        button.HoverExited += (s, e) => e.Color = Color.DarkRed;
#if STEREO
        button.Clicked += (s, e) => QuickConnectPlayer(tankType, teamName);
#else
        button.Clicked += (s, e) => QuickConnectPlayer();
#endif
        _ = new Text(font, Color.White)
        {
            Parent = button.Component,
            Value = "Quick Join",
            TextAlignment = Alignment.Center,
            TextShrink = TextShrinkMode.Width,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Top,
            },
        };
        _ = new Text(font, Color.White)
        {
            Parent = button.Component,
#if STEREO
            Value = $"({tankType}, {teamName})",
#else
            Value = "(player)",
#endif
            TextAlignment = Alignment.Center,
            TextShrink = TextShrinkMode.Width,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Bottom,
            },
        };

        return button;
    }

#endif

    private static Button<Container> CreateButton(
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

        var iconPath = $"Images/Icons/{iconName}.svg";
        var style = Styles.UI.GetButtonStyleWithIcon(text, iconPath, Alignment.Left);
        button.ApplyStyle(style);

        return button;
    }
}
