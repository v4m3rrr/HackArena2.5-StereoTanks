using GameClient.GameSceneComponents.TeamBarComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

#if STEREO

/// <summary>
/// Represents a team bar.
/// </summary>
internal class TeamBar : BaseBar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeamBar"/> class.
    /// </summary>
    /// <param name="team">The team the bar represents.</param>
    /// <param name="isPlayerTeam">Indicates whether the team is the player's team.</param>
    public TeamBar(Team team, bool? isPlayerTeam = null)
    {
        this.Team = team;

        this.Background.Transform.RelativePadding
            = new Vector4(0.04f, 0.2f, 0.04f, 0.14f);

        if (isPlayerTeam is null or true)
        {
            // Score
            _ = new Score(team)
            {
                Parent = this.Container,
                Transform =
                {
                    RelativeSize = new Vector2(0.35f, 0.5f),
                    Alignment = Alignment.Right,
                    RelativePadding = new Vector4(0.1f),
                },
            };
        }

        // Name
        _ = new Name(team)
        {
            Parent = this.Container,
            Transform =
            {
                RelativeSize = new Vector2(0.65f, 0.6f),
                Alignment = Alignment.TopLeft,
                RelativePadding = new Vector4(0.1f),
            },
        };

        // Pings
        _ = new Pings(team)
        {
            Parent = this.Container,
            Transform =
            {
                RelativeSize = new Vector2(0.65f, 0.4f),
                Alignment = Alignment.BottomLeft,
                RelativePadding = new Vector4(0.1f),
            },
        };
    }

    /// <summary>
    /// Gets the team the bar represents.
    /// </summary>
    public Team Team { get; }
}

#endif
