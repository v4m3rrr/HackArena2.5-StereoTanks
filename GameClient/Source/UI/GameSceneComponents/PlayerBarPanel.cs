using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly object syncLock = new();

    private IEnumerable<T> Bars => this.Components.Cast<T>();

    /// <summary>
    /// Refreshes the player bars.
    /// </summary>
    /// <param name="players">The players to display.</param>
    public void Refresh(Dictionary<string, Player> players)
    {
        lock (this.syncLock)
        {
            var newPlayerBars = players.Values
                .Where(player => this.Bars.All(pb => pb.Player != player) && player is not null)
                .Select(player =>
                {
                    var bar = (T)Activator.CreateInstance(typeof(T), player)!;
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

            MonoTanks.InvokeOnMainThread(() =>
            {
                newPlayerBars
                    .SelectMany(pb => pb.GetAllDescendants<TextureComponent>())
                    .Where(x => !x.IsLoaded)
                    .ToList()
                    .ForEach(x => x.Load());

                foreach (T playerBar in this.Bars.ToList())
                {
                    playerBar.Transform.RecalculateIfNeeded();
                }
            });
        }
    }
}
