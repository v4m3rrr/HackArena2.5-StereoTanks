﻿using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;
using SharpDX.Direct2D1.Effects;

namespace GameClient.Scenes.CoopCore;

#pragma warning disable CA1822  // Mark members as static

/// <summary>
/// Represents a join room scene initializer.
/// </summary>
/// <param name="singlePlayer">The join room scene to initialize.</param>
internal class CoopInitializer(Coop coop)
{
    /// <summary>
    /// Creates the room text component.
    /// </summary>
    /// <returns>The created room text component.</returns>
    public LocalizedText CreateRoomText()
    {
        var font = new LocalizedScalableFont(47)
        {
            AutoResize = true,
            Spacing = 10,
        };

        return new LocalizedText(font, Color.White)
        {
            Parent = coop.BaseComponent,

            // Should be LocalizedString()
            Value = new LocalizedString("Labels.CoopRoom"),
            Case = TextCase.Upper,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.Top,
#if STEREO
                RelativeOffset = new Vector2(0.0f, 0.18f),
#else
                RelativeOffset = new Vector2(0.0f, 0.22f),
#endif
            },
        };
    }

    /// <summary>
    /// Gets the address of server.
    /// </summary>
    /// <returns>The address of server.</returns>
    public string GetAddress()
    {
        return CoopData.Address ?? CoopData.DefaultAddress;
    }

    public string GetTeamName()
    {
        return CoopData.TeamName ?? CoopData.DefaultTeamName;
    }

    public TankType GetTankType()
    {
        return CoopData.DefaultTankType;
    }

    public Difficulty GetDifficulty()
    {
        return CoopData.DefaultDifficulty;
    }

    /// <summary>
    /// Creates the base list box component.
    /// </summary>
    /// <returns>The created base list box component.</returns>
    /// <remarks>
    /// The base list box is the container
    /// for all the sections in the join room scene.
    /// </remarks>
    public FlexListBox CreateBaseListBox()
    {
        var container = new FlexListBox()
        {
            Parent = coop.BaseComponent,
            Orientation = MonoRivUI.Orientation.Vertical,
            Spacing = 3,
            Transform =
            {
                Alignment = Alignment.Center,
#if STEREO
                RelativeSize = new Vector2(0.6f, 0.38f),
#else
                RelativeSize = new Vector2(0.6f, 0.4f),
#endif
                RelativeOffset = new Vector2(0.0f, 0.05f),
                MinSize = new Point(620, 100),
            },
        };

        // Background
        _ = new RoundedSolidColor(GameClientCore.ThemeColor, 36)
        {
            Parent = container,
            AutoAdjustRadius = true,
            Opacity = 0.35f,
        };

        return container;
    }

    /// <summary>
    /// Creates a section component with a text input field.
    /// </summary>
    /// <param name="listBox">The list box that will contain the section.</param>
    /// <param name="name">The name of the section.</param>
    /// <param name="charLimit">The character limit for the input field.</param>
    /// <returns>The created section component.</returns>
    public Container CreateSectionWithTextInput(FlexListBox listBox, LocalizedString name, uint charLimit = 16)
    {
        var container = new Container()
        {
            Parent = listBox.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.03f, 0.2f, 0.03f, 0.2f),
            },
        };

        var style = Styles.JoinRoom.GetSectionStyleWithTextInput(name, charLimit);
        container.ApplyStyle(style);

        return container;
    }

#if STEREO

    /// <summary>
    /// Creates a section component with tank type buttons.
    /// </summary>
    /// <param name="listBox">The list box that will contain the section.</param>
    /// <param name="name">The name of the section.</param>
    /// <returns>The created section component with tank type buttons.</returns>
    public Container CreateTankTypeSection(FlexListBox listBox, LocalizedString name)
    {
        var container = new Container()
        {
            Parent = listBox.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.03f, 0.2f, 0.03f, 0.2f),
            },
        };

        var style = Styles.JoinRoom.GetSectionStyle(name);
        container.ApplyStyle(style);

        var inputBackground = container.GetChild<SolidColor>();

        var typeListBox = new FlexListBox();

        var selector = new Selector<TankType>(typeListBox)
        {
            Parent = inputBackground,
            ActiveContainerAlignment = Alignment.Center,
            RelativeHeight = 2f,
            CloseAfterSelect = true,
            Transform =
            {
                IgnoreParentPadding = true,
            },
        };

        selector.ApplyStyle(Styles.Settings.GetSelectorStyle(localized: true));

        selector.ItemSelected += (s, item) =>
            selector.InactiveContainer.GetChild<LocalizedText>()!.Value
                = new LocalizedString($"Other.{item!.Value}");

        var inactiveBackground = selector.InactiveContainer.GetChild<SolidColor>()!;
        inactiveBackground.Opacity = 0f;

        foreach (string typeName in Enum.GetNames(typeof(TankType)))
        {
            var tankType = (TankType)Enum.Parse(typeof(TankType), typeName);

            var button = new Button<Container>(new Container());
            button.ApplyStyle(Styles.Settings.GetSelectorButtonItem(localized: true));
            var text = button.Component.GetChild<LocalizedText>()!;
            text.Value = new LocalizedString($"Other.{typeName}");
            text.Case = TextCase.Upper;

            var item = new Selector<TankType>.Item(button, tankType, typeName);
            selector.AddItem(item);

            button.Clicked += (s, e) => selector.SelectItem(item);
        }

        return container;
    }

    /// <summary>
    /// Creates a section component with tank type buttons.
    /// </summary>
    /// <param name="listBox">The list box that will contain the section.</param>
    /// <param name="name">The name of the section.</param>
    /// <returns>The created section component with tank type buttons.</returns>
    public Container CreateDiffcultySection(FlexListBox listBox, LocalizedString name)
    {
        var container = new Container()
        {
            Parent = listBox.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.03f, 0.2f, 0.03f, 0.2f),
            },
        };

        var style = Styles.JoinRoom.GetSectionStyle(name);
        container.ApplyStyle(style);

        var inputBackground = container.GetChild<SolidColor>();

        var typeListBox = new FlexListBox();

        var selector = new Selector<Difficulty>(typeListBox)
        {
            Parent = inputBackground,
            ActiveContainerAlignment = Alignment.Center,
            RelativeHeight = 3f,
            CloseAfterSelect = true,
            Transform =
            {
                IgnoreParentPadding = true,
            },
        };

        selector.ApplyStyle(Styles.Settings.GetSelectorStyle(localized: true));

        selector.ItemSelected += (s, item) =>
            selector.InactiveContainer.GetChild<LocalizedText>()!.Value
                = new LocalizedString($"Other.{item!.Value}");

        var inactiveBackground = selector.InactiveContainer.GetChild<SolidColor>()!;
        inactiveBackground.Opacity = 0f;

        foreach (string typeName in Enum.GetNames(typeof(Difficulty)))
        {
            var tankType = (Difficulty)Enum.Parse(typeof(Difficulty), typeName);

            var button = new Button<Container>(new Container());
            button.ApplyStyle(Styles.Settings.GetSelectorButtonItem(localized: true));
            var text = button.Component.GetChild<LocalizedText>()!;
            text.Value = new LocalizedString($"Other.{typeName}");
            text.Case = TextCase.Upper;

            var item = new Selector<Difficulty>.Item(button, tankType, typeName);
            selector.AddItem(item);

            button.Clicked += (s, e) => selector.SelectItem(item);
        }

        return container;
    }

#endif

    /// <summary>
    /// Creates the join button component.
    /// </summary>
    /// <returns>The created join button component.</returns>
    public Button<Container> CreateJoinButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = coop.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomRight,
                RelativeOffset = new Vector2(-0.08f, -0.08f),
                RelativeSize = new Vector2(0.2f, 0.07f),
            },
        };

        var text = new LocalizedString("Buttons.Play");
        var iconPath = "Images/Icons/join.svg";
        var style = Styles.UI.GetButtonStyleWithIcon(text, iconPath, Alignment.Right);
        button.ApplyStyle(style);

        // TODO
        button.Clicked += (s, e) => coop.JoinGame();

        return button;
    }

    /// <summary>
    /// Creates the back button component.
    /// </summary>
    /// <returns>The created back button component.</returns>
    public Button<Container> CreateBackButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = coop.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.08f, -0.08f),
                RelativeSize = new Vector2(0.2f, 0.07f),
            },
        };

        button.ApplyStyle(Styles.UI.BackButtonStyle);
        button.Clicked += (s, e) => Scene.ChangeToPreviousOrDefault<MainMenu>();

        return button;
    }
}
