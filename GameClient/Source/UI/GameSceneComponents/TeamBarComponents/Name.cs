using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.TeamBarComponents;

#if STEREO

/// <summary>
/// Represents the name component of a team bar.
/// </summary>
internal class Name : TeamBarComponent
{
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 13)
    {
        AutoResize = true,
        Spacing = 7,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Name"/> class.
    /// </summary>
    /// <param name="team">The team the name belongs to.</param>
    public Name(Team team)
        : base(team)
    {
        _ = new Text(Font, Color.White)
        {
            Parent = this,
            Value = team.Name,
            TextAlignment = Alignment.TopLeft,
            TextShrink = TextShrinkMode.Width,
        };
    }
}

#endif
