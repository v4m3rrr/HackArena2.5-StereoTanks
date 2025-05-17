using GameClient.Scenes.Replay.MatchResultsCore;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI.MatchResultsSceneComponents;

#if HACKATHON

/// <summary>
/// Represents a player slot panel.
/// </summary>
internal class Scoreboard : Component
{
    private readonly AlignedListBox listBox;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scoreboard"/> class.
    /// </summary>
    public Scoreboard()
    {
        this.listBox = new AlignedListBox
        {
            Parent = this,
            Orientation = Orientation.Vertical,
            Spacing = 10,
            ElementsAlignment = Alignment.Center,
        };

        var font = new LocalizedScalableFont(17)
        {
            AutoResize = true,
            Spacing = 5,
        };

        _ = new Text(font, Color.White * 0.8f)
        {
            Parent = this,
            Value = "Wins",
            Case = TextCase.Upper,
            TextAlignment = Alignment.Center,
            Transform =
            {
                RelativeSize = new Vector2(0.08f),
                RelativeOffset = new Vector2(0.9f, 0.18f),
            },
        };
    }

#if STEREO

    /// <summary>
    /// Sets the teams to display.
    /// </summary>
    /// <param name="teams">The teams to set.</param>
    public void SetTeams(List<MatchResultsTeam> teams)
    {
        this.listBox.Clear();

        for (int i = 0; i < teams.Count; i++)
        {
            _ = new TeamSlotPanel(teams[i])
            {
                Parent = this.listBox.ContentContainer,
                Transform =
                {
                    RelativeSize = new Vector2(1.0f, 0.22f),
                },
            };
        }
    }

#else

    /// <summary>
    /// Sets the players to display.
    /// </summary>
    /// <param name="players">The players to set.</param>
    public void SetPlayers(List<MatchResultsPlayer> players)
    {
        this.listBox.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            var isQualified = i < 2;
            _ = new PlayerSlotPanel(players[i], isQualified)
            {
                Parent = this.listBox.ContentContainer,
                Transform =
                {
                    RelativeSize = new Vector2(1.0f, 0.22f),
                },
            };
        }
    }

#endif
}

#endif
