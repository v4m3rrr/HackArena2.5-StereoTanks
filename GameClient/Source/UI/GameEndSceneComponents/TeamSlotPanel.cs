using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI.GameEndSceneComponents;

#if STEREO

/// <summary>
/// Represents a team slot panel.
/// </summary>
internal class TeamSlotPanel : Component
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeamSlotPanel"/> class.
    /// </summary>
    /// <param name="team">The team to represent.</param>
    public TeamSlotPanel(Team team)
        : base()
    {
        var teamColor = new Color(team.Color);

        var background = new RoundedSolidColor(GameClientCore.ThemeColor, 30)
        {
            AutoAdjustRadius = true,
            Parent = this,
            Opacity = 0.45f,
        };

        var teamColorContainer = new Container()
        {
            Parent = this,
            Transform =
            {
                Alignment = Alignment.Left,
                Ratio = new Ratio(1, 1),
            },
        };

        // Team color
        _ = new RoundedSolidColor(teamColor, int.MaxValue)
        {
            Parent = teamColorContainer,
            Transform =
            {
                RelativeSize = new Vector2(0.29f),
                Alignment = Alignment.Center,
            },
        };

        var font = new ScalableFont(Styles.Fonts.Paths.Main, 19)
        {
            AutoResize = true,
            Spacing = 8,
        };

        var teamNameContainer = new Container()
        {
            Parent = this,
            Transform =
            {
                RelativeSize = new Vector2(0.88f, 1f),
                Alignment = Alignment.Right,
            },
        };

        // Teamname
        _ = new Text(font, Color.White)
        {
            Parent = teamNameContainer,
            Value = team.Name,
            Case = TextCase.Upper,
            TextAlignment = Alignment.Left,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Transform =
            {
                RelativeSize = new Vector2(0.9f, 0.6f),
                Alignment = Alignment.Left,
            },
        };

        // Score
        _ = new Text(font, teamColor)
        {
            Parent = background,
            Value = team.Score.ToString(),
            TextAlignment = Alignment.Right,
            Transform =
            {
                RelativeSize = new Vector2(0.9f, 0.6f),
                Alignment = Alignment.Right,
                RelativeOffset = new Vector2(-0.05f, 0.0f),
            },
        };
    }
}

#endif
