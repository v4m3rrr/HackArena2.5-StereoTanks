namespace GameLogic;

#if !STEREO

/// <summary>
/// Handles pickup of items by tanks on the same tile.
/// </summary>
/// <param name="grid">The grid containing the tanks and items.</param>
internal sealed class ItemPickupSystem(Grid grid)
{
    /// <summary>
    /// Checks all tanks and picks up items if possible.
    /// </summary>
    public void Update()
    {
        foreach (var tank in grid.Tanks)
        {
            if (tank.SecondaryItemType is not null)
            {
                continue;
            }

            var item = grid.SecondaryItems.FirstOrDefault(i => i.X == tank.X && i.Y == tank.Y);
            if (item is not null)
            {
                tank.SecondaryItemType = item.Type;
                _ = grid.SecondaryItems.Remove(item);
            }
        }
    }
}

#endif
