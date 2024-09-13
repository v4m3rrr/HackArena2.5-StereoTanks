using System;
using GameClient.GameSceneComponents;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes.GameCore;

/// <summary>
/// Represents the game initializer.
/// </summary>
/// <param name="game">The game scene to initialize.</param>
internal class GameInitializer(Game game)
{
    /// <summary>
    /// Creates a grid component.
    /// </summary>
    /// <returns>The created grid component.</returns>
    public GridComponent CreateGridComponent()
    {
        return new GridComponent()
        {
            IsEnabled = false,
            Parent = game.BaseComponent,
            Transform =
            {
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
                RelativeSize = new Vector2(0.8f),
                RelativeOffset = new Vector2(0.0f, 0.03f),
            },
        };
    }

    /// <summary>
    /// Creates player bar panels.
    /// </summary>
    /// <returns>
    /// A tuple containing the player identity bar panel
    /// and the player stats bar panel.
    /// </returns>
    public Tuple<PlayerBarPanel<PlayerIdentityBar>, PlayerBarPanel<PlayerStatsBar>> CreatePlayerBarPanels()
    {
        var boxRelativeSize = new Vector2(0.23f, 1.0f);
        var boxRelativePadding = new Vector4(0.05f);
        var boxSpacing = 8;
        var boxElementsAlignment = Alignment.Center;

        var identityBox = new PlayerBarPanel<PlayerIdentityBar>()
        {
            Parent = game.BaseComponent,
            ElementsAlignment = boxElementsAlignment,
            Spacing = boxSpacing,
            Transform =
            {
                Alignment = Alignment.Left,
                RelativeSize = boxRelativeSize,
                RelativePadding = boxRelativePadding,
            },
        };

        var statsBox = new PlayerBarPanel<PlayerStatsBar>()
        {
            Parent = game.BaseComponent,
            ElementsAlignment = boxElementsAlignment,
            Spacing = boxSpacing,
            Transform =
            {
                Alignment = Alignment.Right,
                RelativeSize = boxRelativeSize,
                RelativePadding = boxRelativePadding,
            },
        };

        return new(identityBox, statsBox);
    }

    /// <summary>
    /// Creates a timer.
    /// </summary>
    /// <returns>The created timer.</returns>
    public Timer CreateTimer()
    {
        return new Timer()
        {
            Parent = game.BaseComponent,
            Transform =
            {
                Alignment = Alignment.TopLeft,
                RelativeSize = new Vector2(0.1f, 0.045f),
                RelativeOffset = new Vector2(0.02f, 0.06f),
            },
        };
    }

    /// <summary>
    /// Creates a match name.
    /// </summary>
    /// <returns>The created match name.</returns>
    public Text CreateMatchName()
    {
        var font = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 15);
        return new Text(font, Color.White)
        {
            Parent = game.BaseComponent,
            Value = "Match Name",
            Case = TextCase.Upper,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Spacing = 10,
            Transform =
            {
                Alignment = Alignment.Top,
                RelativeOffset = new Vector2(0.0f, 0.03f),
            },
        };
    }
}
