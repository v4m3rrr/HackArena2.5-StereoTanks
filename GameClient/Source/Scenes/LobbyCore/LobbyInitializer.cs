using System.Collections.Generic;
using GameClient.Networking;
using GameClient.UI.LobbySceneComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes.LobbyCore;

/// <summary>
/// Represents the initializer for the <see cref="Lobby"/> scene.
/// </summary>
/// <param name="lobby">The lobby to initialize.</param>
internal class LobbyInitializer(Lobby lobby)
{
#if HACKATHON

    /// <summary>
    /// Creates a match name component.
    /// </summary>
    /// <returns>The created match name component.</returns>
    public Text CreateMatchName()
    {
        var font = new ScalableFont(Styles.Fonts.Paths.Main, 35)
        {
            AutoResize = true,
            Spacing = 10,
        };

        return new Text(font, Color.White)
        {
            Parent = lobby.BaseComponent,
            Value = "Lobby",
            Case = TextCase.Upper,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.Top,
                RelativeOffset = new Vector2(0.0f, 0.15f),
            },
        };
    }

#endif

    /// <summary>
    /// Creates a join code component.
    /// </summary>
    /// <returns>The created join code component.</returns>
    public Text CreateJoinCode()
    {
        var font = new ScalableFont(Styles.Fonts.Paths.Main, 14)
        {
            AutoResize = true,
            Spacing = 5,
        };

        return new Text(font, Color.White)
        {
            Parent = lobby.BaseComponent,
            TextShrink = TextShrinkMode.Width,
            TextAlignment = Alignment.Center,
            Transform =
            {
                RelativeSize = new Vector2(0.2f, 0.05f),
                Alignment = Alignment.Top,
                RelativeOffset = new Vector2(0.0f, 0.22f),
            },
        };
    }

    /// <summary>
    /// Creates a leave button.
    /// </summary>
    /// <returns>The created leave button.</returns>
    public Button<Container> CreateLeaveButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = lobby.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.08f, -0.08f),
                RelativeSize = new Vector2(0.2f, 0.07f),
            },
        };

        var text = new LocalizedString("Buttons.Leave");
        var iconPath = "Images/Icons/leave.svg";
        var style = Styles.UI.GetButtonStyleWithIcon(text, iconPath, Alignment.Left);
        button.ApplyStyle(style);

        button.Clicked += (s, e) =>
        {
            _ = ServerConnection.CloseAsync("Leave lobby");
            Scene.Change<MainMenu>();
        };

        return button;
    }

#if STEREO

    public List<TeamSlotPanel> CreateTeamSlotPanels()
    {
        var container = new Container()
        {
            Parent = lobby.BaseComponent,
            Transform =
            {
                RelativeSize = new Vector2(0.7f, 0.42f),
                Alignment = Alignment.Center,
                RelativeOffset = new Vector2(0.0f, 0.05f),
            },
        };

        var leftListBox = new TeamSlotPanel()
        {
            Parent = container,
            Orientation = MonoRivUI.Orientation.Vertical,
            Transform =
            {
                RelativePadding = new Vector4(0.01f),
                RelativeSize = new Vector2(0.49f, 1f),
                Alignment = Alignment.Left,
            },
        };

        var rightListBox = new TeamSlotPanel()
        {
            Parent = container,
            Orientation = MonoRivUI.Orientation.Vertical,
            Transform =
            {
                RelativePadding = new Vector4(0.01f),
                RelativeSize = new Vector2(0.49f, 1f),
                Alignment = Alignment.Right,
            },
        };

        return [leftListBox, rightListBox];
    }

#else

    /// <summary>
    /// Creates player slot panels.
    /// </summary>
    /// <returns>The created player slot panels.</returns>
    public List<PlayerSlotPanel> CreatePlayerSlotPanels()
    {
        var container = new Container()
        {
            Parent = lobby.BaseComponent,
            Transform =
            {
                RelativeSize = new Vector2(0.7f, 0.42f),
                Alignment = Alignment.Center,
                RelativeOffset = new Vector2(0.0f, 0.05f),
            },
        };

        const float relativeSpacing = 0.002f;

        var upperListBox = new FlexListBox()
        {
            Parent = container,
            Orientation = MonoRivUI.Orientation.Horizontal,
            Spacing = (int)(0.01f * ScreenController.Width),
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.46f),
                Alignment = Alignment.Top,
            },
        };

        var bottomListBox = new FlexListBox()
        {
            Parent = container,
            Orientation = MonoRivUI.Orientation.Horizontal,
            Spacing = (int)(relativeSpacing * ScreenController.Width),
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.46f),
                Alignment = Alignment.Bottom,
            },
        };

        GameSettings.ResolutionChanging += (s, e) =>
        {
            upperListBox.Spacing = bottomListBox.Spacing
                = (int)(relativeSpacing * ScreenController.Width);
        };

        List<FlexListBox> listBoxes = [
            upperListBox,
            bottomListBox,
        ];

        var panels = new List<PlayerSlotPanel>();

        for (int i = 0; i < 4; i++)
        {
            var panel = new PlayerSlotPanel()
            {
                Parent = listBoxes[i / 2].ContentContainer,
                IsEnabled = false,
            };

            panels.Add(panel);
        }

        return panels;
    }

#endif

    /// <summary>
    /// Creates a continue button to start the replay.
    /// </summary>
    /// <returns>The created continue button.</returns>
    public Button<Container> CreateContinueButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = lobby.BaseComponent,
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

        button.Clicked += (s, e) => Scene.ChangeWithoutStack<Game>(lobby.ReplayArgs);

        return button;
    }
}
