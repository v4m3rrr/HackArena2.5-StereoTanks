#if HACKATHON && STEREO

using GameClient.Scenes.Replay.MatchResultsCore;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI.MatchResultsSceneComponents;

/// <summary>
/// Represents a team slot panel.
/// </summary>
/// <remarks>
/// The team slot panel displays the team name,
/// score on the match results screen.
/// </remarks>
internal class TeamSlotPanel : Component
{
    private readonly RoundedSolidColor background;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamSlotPanel"/> class.
    /// </summary>
    /// <param name="team">The team to display.</param>
    public TeamSlotPanel(MatchResultsTeam team)
    {
        var color = new Color(team.Color);

        this.background = new RoundedSolidColor(GameClientCore.ThemeColor, 20)
        {
            AutoAdjustRadius = true,
            Parent = this,
            Opacity = 0.45f,
        };

        var font = new ScalableFont(Styles.Fonts.Paths.Main, 19)
        {
            AutoResize = true,
            Spacing = 8,
        };

        var nameContainer = new Container()
        {
            Parent = this,
            Transform =
            {
                RelativeSize = new Vector2(0.62f, 1f),
                RelativeOffset = new Vector2(0.04f, 0.0f),
                Alignment = Alignment.Left,
            },
        };

        // Team name
        _ = new Text(font, Color.White)
        {
            Parent = nameContainer,
            Value = team.TeamName,
            Case = TextCase.Upper,
            TextAlignment = Alignment.Left,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Transform =
            {
                RelativeSize = new Vector2(0.9f, 0.6f),
                Alignment = Alignment.Left,
            },
        };

        // Rounds won
        _ = new Text(font, color)
        {
            Parent = this.background,
            Value = team.RoundsWon.ToString(),
            TextAlignment = Alignment.Right,
            Transform =
            {
                RelativeSize = new Vector2(0.9f, 0.6f),
                Alignment = Alignment.Right,
                RelativeOffset = new Vector2(-0.06f, 0.0f),
            },
        };
    }
}

#endif
