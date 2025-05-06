using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.TeamBarComponents;

#if STEREO

/// <summary>
/// Represents the score component of a team bar.
/// </summary>
internal class Score : TeamBarComponent
{
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 12)
    {
        AutoResize = true,
        Spacing = 5,
    };

    private readonly Text text;

    /// <summary>
    /// Initializes a new instance of the <see cref="Score"/> class.
    /// </summary>
    /// <param name="team">The team the score belongs to.</param>
    public Score(Team team)
        : base(team)
    {
        this.text = new Text(Font, Color.White)
        {
            Parent = this,
            Value = team.Score.ToString(),
            TextAlignment = Alignment.Right,
        };
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.text.Value = this.Team.Score.ToString();

        base.Update(gameTime);
    }
}

#endif
