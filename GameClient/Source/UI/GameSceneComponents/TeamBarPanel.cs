using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

#if STEREO

/// <summary>
/// Represents a player bar panel.
/// </summary>
internal class TeamBarPanel : AlignedListBox
{
    private TeamBar? TeamBar => this.ContentContainer.GetChild<TeamBar>();

    private IEnumerable<TeamPlayerBar> PlayerBars => this.ContentContainer.GetAllChildren<TeamPlayerBar>();

    /// <summary>
    /// Refreshes the player bars.
    /// </summary>
    /// <param name="team">The team to display.</param>
    /// <param name="teamName">The team name for whom the bar should be displayed.</param>
    public void Refresh(Team team, string? teamName = null)
    {
        if (this.TeamBar is null)
        {
            var isPlayerTeam = Scenes.Game.PlayerId is null
                || team.Players.Any(p => p.Id == Scenes.Game.PlayerId);

            var teamBar = new TeamBar(team, isPlayerTeam)
            {
                Parent = this.ContentContainer,
                Transform =
                {
                    RelativeSize = new Vector2(1f, 0.2f),
                    Alignment = Alignment.Top,
                    Ratio = new Ratio(340, 120),
                    IgnoreParentPadding = true,
                },
            };

            teamBar.GetAllDescendants<TextureComponent>()
                .Where(x => !x.IsLoaded)
                .ToList()
                .ForEach(x => x.Load());
        }

        var newPlayerBars = team.Players
            .Where(p => this.PlayerBars.All(pb => !pb.Player.Equals(p))
                && p is not null
                && (teamName is null || p.Team.Name == teamName))
            .Select(p =>
            {
                return new TeamPlayerBar(p)
                {
                    Parent = this.ContentContainer,
                    Transform =
                    {
                        RelativeSize = new Vector2(1f, 0.2f),
                        Alignment = Alignment.Top,
                        Ratio = new Ratio(340, 120),
                        IgnoreParentPadding = true,
                    },
                };
            })
            .ToList();

        foreach (TeamPlayerBar playerBar in this.PlayerBars.ToList())
        {
            if (!team.Players.Contains(playerBar.Player))
            {
                playerBar.Parent = null;
            }
        }

        newPlayerBars
            .SelectMany(pb => pb.GetAllDescendants<TextureComponent>())
            .Where(x => !x.IsLoaded)
            .ToList()
            .ForEach(x => x.Load());

        this.ForceUpdate();
    }
}

#endif
