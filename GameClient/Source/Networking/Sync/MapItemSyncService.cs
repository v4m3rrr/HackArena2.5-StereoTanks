namespace GameClient.Networking;

#if !STEREO

/// <summary>
/// Synchronizes laser sprites with the current game logic state.
/// </summary>
/// <param name="parent">The grid component containing the logic state.</param>
/// <param name="itemSprites">The list of item sprites currently rendered.</param>
internal sealed class MapItemSyncService(
    GridComponent parent,
    List<Sprites.SecondaryItem> itemSprites)
    : ISyncService
{
    /// <inheritdoc/>
    public void Sync()
    {
        itemSprites.Clear();
        foreach (var item in parent.Logic.SecondaryItems)
        {
            var sprite = itemSprites.FirstOrDefault(l => l.Logic.Equals(item));
            if (sprite == null)
            {
                itemSprites.Add(new Sprites.SecondaryItem(item, parent));
            }
        }

        _ = itemSprites.RemoveAll(l => !parent.Logic.SecondaryItems.Contains(l.Logic));
    }
}

#endif
