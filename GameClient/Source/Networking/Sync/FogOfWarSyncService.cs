using GameClient.Sprites;
using Microsoft.Xna.Framework;

namespace GameClient.Networking;

/// <summary>
/// Synchronizes fog of war visualizations with the visibility state of tanks.
/// </summary>
/// <param name="parent">The grid component containing game state information.</param>
/// <param name="fogOfWarSprites">The list of fog-of-war sprites associated with tanks.</param>
internal class FogOfWarSyncService(GridComponent parent, List<FogOfWar> fogOfWarSprites)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        foreach (var tank in parent.Logic.Tanks)
        {
            if (tank.VisibilityGrid is null)
            {
                continue;
            }

            var sprite = fogOfWarSprites.FirstOrDefault(m => m.Tank.Equals(tank));
            if (sprite == null)
            {
                sprite = new FogOfWar(tank, parent, new Color(tank.Owner.Color));
                fogOfWarSprites.Add(sprite);
            }
            else
            {
                sprite.VisibilityGrid = tank.VisibilityGrid;
            }
        }

        _ = fogOfWarSprites.RemoveAll(m => !parent.Logic.Tanks.Any(l => l.Equals(m.Tank)));
    }
}
