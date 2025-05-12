using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

/// <summary>
/// Represents a player bar panel.
/// </summary>
/// <typeparam name="T">The type of player bar to display.</typeparam>
internal class PlayerBarPanel<T> : AlignedListBox
    where T : PlayerBar
{
    private IEnumerable<T> Bars => this.Components.Cast<T>();

    /// <summary>
    /// Refreshes the player bars.
    /// </summary>
    /// <param name="players">The players to display.</param>
    /// <param name="playerId">The player's id for whom the bar should be displayed.</param>
    public void Refresh(Dictionary<string, Player> players, string? playerId = null)
    {
        var newPlayerBars = players.Values
            .Where(p => this.Bars.All(pb => pb.Player != p)
                && p is not null
                && (playerId is null || p.Id == playerId))
            .Select(p =>
            {
                var bar = (T)Activator.CreateInstance(typeof(T), p)!;
                bar.Parent = this.ContentContainer;
                bar.Transform.RelativeSize = new Vector2(1f, 0.2f);
                bar.Transform.Alignment = Alignment.Top;
                bar.Transform.Ratio = new Ratio(340, 120);
                bar.Transform.IgnoreParentPadding = true;
                return bar;
            })
            .ToList();

        foreach (T playerBar in this.Bars.ToList())
        {
            if (!players.ContainsValue(playerBar.Player))
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
