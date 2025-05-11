namespace GameClient.Networking;

/// <summary>
/// Synchronizes laser sprites with the current game logic state.
/// </summary>
/// <param name="parent">The grid component containing the logic state.</param>
/// <param name="laserSprites">The list of laser sprites currently rendered.</param>
internal sealed class LaserSyncService(
    GridComponent parent,
    List<Sprites.Laser> laserSprites)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        foreach (var laser in parent.Logic.Lasers)
        {
            var sprite = laserSprites.FirstOrDefault(l => l.Logic.Equals(laser));
            if (sprite == null)
            {
                laserSprites.Add(new Sprites.Laser(laser, parent));
            }
        }

        _ = laserSprites.RemoveAll(l => !parent.Logic.Lasers.Contains(l.Logic));
    }
}
