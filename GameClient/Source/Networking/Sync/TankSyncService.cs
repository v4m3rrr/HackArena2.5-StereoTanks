namespace GameClient.Networking;

/// <summary>
/// Synchronizes tank sprites with the current tank state in the game logic.
/// </summary>
/// <param name="parent">The grid component containing tank state.</param>
/// <param name="tankSprites">The list of tank sprites currently rendered.</param>
internal sealed class TankSyncService(GridComponent parent, List<Sprites.Tank> tankSprites)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        foreach (var tank in parent.Logic.Tanks)
        {
            var sprite = tankSprites.FirstOrDefault(t => t.Logic.Equals(tank));
            if (sprite == null)
            {
                sprite = new Sprites.Tank(tank, parent);
                tankSprites.Add(sprite);
            }
            else
            {
                sprite.UpdateLogic(tank);
            }
        }

        _ = tankSprites.RemoveAll(s => !parent.Logic.Tanks.Any(t => t.Equals(s.Logic)));
    }
}
