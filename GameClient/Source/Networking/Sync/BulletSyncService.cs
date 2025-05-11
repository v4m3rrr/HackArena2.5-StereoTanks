using GameLogic;

namespace GameClient.Networking;

/// <summary>
/// Synchronizes bullet sprites with the current game logic state.
/// </summary>
/// <param name="parent">The grid component containing the logic state.</param>
/// <param name="bulletSprites">The list of bullet sprites currently rendered.</param>
internal sealed class BulletSyncService(
    GridComponent parent,
    List<Sprites.Bullet> bulletSprites)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        foreach (var bullet in parent.Logic.Bullets)
        {
            var sprite = bulletSprites.FirstOrDefault(b => b.Logic.Equals(bullet));
            if (sprite == null)
            {
                sprite = bullet is DoubleBullet dbl
                    ? new Sprites.DoubleBullet(dbl, parent)
                    : new Sprites.Bullet(bullet, parent);

                bulletSprites.Add(sprite);
            }
            else
            {
                sprite.UpdateLogic(bullet);
            }
        }

        _ = bulletSprites.RemoveAll(b => !parent.Logic.Bullets.Contains(b.Logic));
    }
}
