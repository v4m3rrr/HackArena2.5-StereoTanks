namespace GameClient.Networking;

/// <summary>
/// Synchronizes zone sprites with the current zone state in the game logic.
/// </summary>
/// <param name="parent">The grid component containing zone state.</param>
/// <param name="zoneSprites">The list of zone sprites currently rendered.</param>
internal sealed class ZoneSyncService(
    GridComponent parent,
    List<Sprites.Zone> zoneSprites)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        foreach (var zone in parent.Logic.Zones)
        {
            var sprite = zoneSprites.FirstOrDefault(z => z.Logic.Equals(zone));
            if (sprite == null)
            {
                zoneSprites.Add(new Sprites.Zone(zone, parent));
            }
            else
            {
                sprite.UpdateLogic(zone);
            }
        }

        _ = zoneSprites.RemoveAll(z => !parent.Logic.Zones.Any(logic => logic.Equals(z.Logic)));
    }
}
