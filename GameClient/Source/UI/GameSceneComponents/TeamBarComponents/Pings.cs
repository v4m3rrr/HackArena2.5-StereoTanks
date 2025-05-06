using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.TeamBarComponents;

#if STEREO

/// <summary>
/// Represents a component that displays the pings of players in a team.
/// </summary>
internal class Pings : TeamBarComponent
{
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 8)
    {
        AutoResize = true,
        Spacing = 7,
    };

    private readonly List<Text> texts = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Pings"/> class.
    /// </summary>
    /// <param name="team">The team the players' pings belong to.</param>
    public Pings(Team team)
        : base(team)
    {
        var left = new Text(Font, new Color(team.Color))
        {
            Parent = this,
            Value = $"-",
            TextAlignment = Alignment.TopRight,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Spacing = 3,
            Transform =
            {
                RelativeSize = new Vector2(0.5f, 1f),
                Alignment = Alignment.Left,
            },
        };

        var right = new Text(Font, Color.White * 0.5f)
        {
            Parent = this,
            Value = $"-",
            TextAlignment = Alignment.TopLeft,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Spacing = 3,
            Transform =
            {
                RelativeSize = new Vector2(0.5f, 1f),
                Alignment = Alignment.Right,
            },
        };

        this.texts.Add(left);
        this.texts.Add(right);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        int textIndex = 0;
        foreach (var (text, player) in this.texts.Zip(this.Team.Players))
        {
            var ping = player.Ping;
            text.IsEnabled = true;
            text.Value = ping < 1 ? "<1 MS" : $"{ping} MS";
            textIndex++;
        }

        foreach (var text in this.texts.Skip(textIndex))
        {
            text.IsEnabled = false;
        }

        base.Update(gameTime);
    }
}

#endif
