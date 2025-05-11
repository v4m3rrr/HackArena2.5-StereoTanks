namespace GameClient.Networking;

/// <summary>
/// Synchronizes mine sprites with the current mine state in the game logic.
/// </summary>
/// <param name="parent">The grid component containing logic state.</param>
/// <param name="mineSprites">The list of mine sprites currently rendered.</param>
internal sealed class MineSyncService(
    GridComponent parent,
    List<Sprites.Mine> mineSprites)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        foreach (var mine in parent.Logic.Mines)
        {
            var sprite = mineSprites.FirstOrDefault(m => m.Logic.Equals(mine));
            if (sprite == null)
            {
                mineSprites.Add(new Sprites.Mine(mine, parent));
            }
            else
            {
                sprite.UpdateLogic(mine);
            }
        }

        _ = mineSprites.RemoveAll(m =>
            m.IsFullyExploded || !parent.Logic.Mines.Any(l => l.Equals(m.Logic)));
    }
}
