namespace GameLogic;

#if !STEREO

/// <summary>
/// Handles item spawning logic.
/// </summary>
/// <param name="grid">The grid instance.</param>
internal sealed class ItemSpawnSystem(Grid grid)
{
    /// <summary>
    /// Generates a new item on the map if possible.
    /// </summary>
    /// <returns>
    /// The spawned item, or <see langword="null"/> if spawn failed.
    /// </returns>
    public SecondaryItem? GenerateNewItem()
    {
        if (grid.SecondaryItems.Count > 2 * grid.Dim)
        {
            return null;
        }

        var itemWeights = new Dictionary<SecondaryItemType, double>
        {
            { SecondaryItemType.Laser, 0.09 },
            { SecondaryItemType.DoubleBullet, 0.9 },
            { SecondaryItemType.Radar, 0.3 },
            { SecondaryItemType.Mine, 0.5 },
        };

        double totalWeight = itemWeights.Values.Sum() + 99.5;
        double randomValue = grid.Random.NextDouble() * totalWeight;

        var selectedType = GetRandomItemByWeight(itemWeights, randomValue);
        if (selectedType is null)
        {
            return null;
        }

        var point = this.FindFreeTileForItem();
        if (point is null)
        {
            return null;
        }

        var item = new SecondaryItem(point.Value.X, point.Value.Y, selectedType.Value);
        grid.SecondaryItems.Add(item);
        return item;
    }

    private static SecondaryItemType? GetRandomItemByWeight(
        Dictionary<SecondaryItemType, double> weights,
        double roll)
    {
        double cumulative = 0;
        foreach (var pair in weights)
        {
            cumulative += pair.Value;
            if (roll <= cumulative)
            {
                return pair.Key;
            }
        }

        return null;
    }

    private (int X, int Y)? FindFreeTileForItem()
    {
        int attempts = 200;
        int x, y;

        do
        {
            x = grid.Random.Next(grid.Dim);
            y = grid.Random.Next(grid.Dim);

            bool occupied = grid.GetCellObjects(x, y).Any()
                         || grid.Zones.Any(z => z.Contains(x, y))
                         || grid.Tanks.Any(t => t.VisibilityGrid?[x, y] == true);

            if (!occupied)
            {
                return (x, y);
            }

            attempts--;
        }
        while (attempts > 0);

        return null;
    }
}

#endif
