namespace GameLogic;

#if !STEREO

/// <summary>
/// Handles pickup of items by tanks on the same tile.
/// </summary>
/// <param name="grid">The grid instance.</param>
internal sealed class ItemDropSystem(Grid grid)
{
    /// <summary>
    /// Tries to drop an item from the tank onto a nearby tile.
    /// </summary>
    /// <param name="tank">The tank from which to drop the item.</param>
    public void TryDropItem(Tank tank)
    {
        if (tank.SecondaryItemType is not { } itemType)
        {
            return;
        }

        var dropTile = this.FindDropTileNear(tank.X, tank.Y);
        tank.SecondaryItemType = null;

        if (dropTile is null)
        {
            return;
        }

        var (x, y) = dropTile.Value;
        grid.SecondaryItems.Add(new SecondaryItem(x, y, itemType));
    }

    /// <summary>
    /// Finds a nearby free tile to drop an item, starting from the given position.
    /// </summary>
    /// <param name="originX">The origin X coordinate.</param>
    /// <param name="originY">The origin Y coordinate.</param>
    /// <returns>
    /// The coordinates of a suitable tile, or <see langword="null"/> if none found.
    /// </returns>
    private (int X, int Y)? FindDropTileNear(int originX, int originY)
    {
        var visited = new bool[grid.Dim, grid.Dim];
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((originX, originY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            if (!grid.IsCellWithinBounds(x, y)
                || visited[x, y]
                || grid.WallGrid[x, y] is not null)
            {
                continue;
            }

            visited[x, y] = true;

            if (!grid.GetCellObjects(x, y).Any(o => o is SecondaryItem))
            {
                return (x, y);
            }

            queue.Enqueue((x + 1, y));
            queue.Enqueue((x - 1, y));
            queue.Enqueue((x, y + 1));
            queue.Enqueue((x, y - 1));
        }

        return null;
    }
}

#endif
