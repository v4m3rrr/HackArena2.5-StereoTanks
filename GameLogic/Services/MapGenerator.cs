using System.Diagnostics;
using System.Drawing;

namespace GameLogic;

/// <summary>
/// Represents a map generator.
/// </summary>
/// <param name="dimension">The dimension of the grid.</param>
/// <param name="seed">The seed for the random number generator.</param>
internal class MapGenerator(int dimension, int seed)
{
    private readonly int dim = dimension;
    private readonly Random random = new(seed);

    /// <summary>
    /// Occurs when the generation warning is raised.
    /// </summary>
    public event EventHandler<string>? GenerationWarning;

    /// <summary>
    /// Generates a new wall grid.
    /// </summary>
    /// <param name="zones">The zones to avoid generating walls in.</param>
    /// <returns>
    /// The 2D array representing the wall grid,
    /// where <c>true</c> means a wall
    /// and <c>false</c> means an empty space.
    /// </returns>
    public Wall?[,] GenerateWalls(IEnumerable<Zone> zones)
    {
        var grid = new bool[this.dim, this.dim];

        int maxInnerWalls = this.dim * this.dim * 75 / 100;
        int innerWalls = 0;

        while (innerWalls < maxInnerWalls)
        {
            int x = this.random.Next(0, this.dim);
            int y = this.random.Next(0, this.dim);

            grid[x, y] = true;
            innerWalls++;
        }

        this.RemoveSomeWallsFromZones(grid, zones);
        this.ConnectEnclosedAreas(grid);
        this.RemoveSomeWallsFromZones(grid, zones);

#if STEREO
        var penetrableMask = this.GeneratePenetrableMaskWithFalloff(grid, zones);
#endif

        var wallGrid = new Wall?[this.dim, this.dim];
        for (int x = 0; x < this.dim; x++)
        {
            for (int y = 0; y < this.dim; y++)
            {
                wallGrid[x, y] = grid[x, y]
                    ? new Wall(x, y)
                    {
#if STEREO
                        Type = penetrableMask[x, y]
                            ? WallType.Penetrable
                            : WallType.Solid,
#endif
                    }
                    : null;
            }
        }

        return wallGrid;
    }

    /// <summary>
    /// Generates a list of zones.
    /// </summary>
    /// <returns>The list of zones.</returns>
    public List<Zone> GenerateZones()
    {
        const int height = 4;
        const int width = 4;
#if STEREO
        const int count = 1;
#else
        const int count = 2;
#endif
        const int maxAttempts = 10000;

        var zones = new List<Zone>();
        var length = Math.Max(height, width);
        var minDistance = ((this.dim - length) / 2) - length;

        bool IsOverlapping(int x, int y) => zones.Any(z =>
            x < z.X + z.Width &&
            x + width > z.X &&
            y < z.Y + z.Height &&
            y + height > z.Y);

        bool IsTooClose(int x, int y) => zones.Any(z =>
            (x + width + minDistance > z.X && x < z.X) ||
            (x < z.X + z.Width + minDistance && x > z.X) ||
            (y + height + minDistance > z.Y && y < z.Y) ||
            (y < z.Y + z.Height + minDistance && y > z.Y));

        for (int i = 0; i < count; i++)
        {
            int x, y;
            int attempts = 0;

            do
            {
                x = this.random.Next(1, this.dim - width);
                y = this.random.Next(1, this.dim - height);

                if (attempts++ >= maxAttempts)
                {
                    this.GenerationWarning?.Invoke(
                        this,
                        $"Max attempts of generating zones reached. Generated: {zones.Count} from {count}");

                    return zones;
                }
            } while (IsOverlapping(x, y) || IsTooClose(x, y));

            var index = (char)(i + 65);

            if (index >= 'Z')
            {
                Debug.Fail("Too many zones. Index out of range.");
                break;
            }

            var zone = new Zone(x, y, width, height, index);
            zones.Add(zone);
        }

        return zones;
    }

#if STEREO

    private bool[,] GeneratePenetrableMaskWithFalloff(
        bool[,] grid,
        IEnumerable<Zone> zones,
        float innerChance = 0.66f,
        float minChance = 0.24f)
    {
        var mask = new bool[this.dim, this.dim];

        // Precompute all distances
        int[,] minDistances = new int[this.dim, this.dim];
        int maxDistance = 0;

        for (int x = 0; x < this.dim; x++)
        {
            for (int y = 0; y < this.dim; y++)
            {
                int min = zones.Min(z => z.ManhattanDistanceTo(x, y));
                minDistances[x, y] = min;
                maxDistance = Math.Max(maxDistance, min);
            }
        }

        for (int x = 0; x < this.dim; x++)
        {
            for (int y = 0; y < this.dim; y++)
            {
                if (!grid[x, y])
                {
                    continue;
                }

                int distance = minDistances[x, y];
                float t = maxDistance > 0 ? (float)distance / maxDistance : 0f;
                float chance = (innerChance * (1 - t)) + (minChance * t);

                if (this.random.NextSingle() < chance)
                {
                    mask[x, y] = true;
                }
            }
        }

        return mask;
    }

#endif

    private void RemoveSomeWallsFromZones(bool[,] grid, IEnumerable<Zone> zones)
    {
        foreach (var zone in zones)
        {
            var remainingWalls = zone.Width * zone.Height * this.random.Next(10, 20) / 100f;
            var walls = new List<Point>();

            for (int i = zone.X; i < zone.X + zone.Width; i++)
            {
                for (int j = zone.Y; j < zone.Y + zone.Height; j++)
                {
                    if (grid[i, j])
                    {
                        walls.Add(new Point(i, j));
                    }
                }
            }

            while (walls.Count > remainingWalls)
            {
                var wall = walls[this.random.Next(walls.Count)];
                grid[wall.X, wall.Y] = false;
                _ = walls.Remove(wall);
            }
        }
    }

    private List<List<Point>> IdentifyEnclosedAreas(bool[,] grid)
    {
        var visited = new bool[this.dim, this.dim];
        var enclosedAreas = new List<List<Point>>();

        for (int i = 0; i < this.dim; i++)
        {
            for (int j = 0; j < this.dim; j++)
            {
                if (!visited[i, j] && !grid[i, j])
                {
                    var enclosedArea = this.FloodFill(grid, visited, i, j);
                    enclosedAreas.Add(enclosedArea);
                }
            }
        }

        return enclosedAreas;
    }

    private void SealSmallEnclosedAreas(bool[,] grid, List<List<Point>> enclosedAreas)
    {
        foreach (var area in enclosedAreas.ToList())
        {
            if (!enclosedAreas.Contains(area))
            {
                continue;
            }

            var paths = this.FindPathsToNearbyAreas(grid, area);
            paths.Sort((a, b) => a.Count.CompareTo(b.Count));

            var path = paths.First();
            if (path.Count - 1 > area.Count)
            {
                _ = enclosedAreas.Remove(area);
                foreach (var point in area)
                {
                    grid[point.X, point.Y] = true;
                }
            }
        }
    }

    private void BreakThroughClosedAreas(bool[,] grid, List<List<Point>> enclosedAreas)
    {
        if (enclosedAreas.Count > 1)
        {
            foreach (var area in enclosedAreas)
            {
                var paths = this.FindPathsToNearbyAreas(grid, area);
                paths.Sort((a, b) => a.Count.CompareTo(b.Count));

                foreach (var path in paths.Take(area.Count >> 4 | 3))
                {
                    foreach (var point in path)
                    {
                        grid[point.X, point.Y] = false;
                    }
                }
            }
        }
    }

    private void RemoveWallsWithConstraints(bool[,] grid, int attempts)
    {
        for (int i = 0; i < attempts; i++)
        {
            var x = this.random.Next(this.dim);
            var y = this.random.Next(this.dim);

            if (
                (x - 1 >= 0 && !grid[x - 1, y]) ||
                (x + 1 < this.dim && !grid[x + 1, y]) ||
                (y - 1 >= 0 && !grid[x, y - 1]) ||
                (y + 1 < this.dim && !grid[x, y + 1]))
            {
                grid[x, y] = false;
            }
        }
    }

    private void AddWallsWithConstraints(bool[,] grid, int attempts)
    {
        for (int i = 0; i < attempts; i++)
        {
            var x = this.random.Next(1, this.dim - 1);
            var y = this.random.Next(1, this.dim - 1);

            var c = new List<bool>()
            {
                grid[x - 1, y],
                grid[x + 1, y],
                grid[x, y - 1],
                grid[x, y + 1],
                grid[x - 1, y - 1],
                grid[x + 1, y - 1],
                grid[x - 1, y + 1],
                grid[x + 1, y + 1],
            }.Count(c => c);

            if (c <= 0)
            {
                grid[x, y] = true;
            }
        }
    }

    private void ConnectEnclosedAreas(bool[,] grid)
    {
        var visited = new bool[this.dim, this.dim];
        var enclosedAreas = new List<List<Point>>();

        enclosedAreas = this.IdentifyEnclosedAreas(grid);

        do
        {
            enclosedAreas.Sort((a, b) => b.Count.CompareTo(a.Count));
            this.SealSmallEnclosedAreas(grid, enclosedAreas);
            this.BreakThroughClosedAreas(grid, enclosedAreas);
            enclosedAreas = this.IdentifyEnclosedAreas(grid);
        } while (enclosedAreas.Count > 1);

        this.RemoveWallsWithConstraints(grid, this.dim * this.dim / 2);
        this.AddWallsWithConstraints(grid, this.dim * this.dim / 2);
    }

    private List<List<Point>> FindPathsToNearbyAreas(bool[,] grid, List<Point> area)
    {
        var paths = new List<List<Point>>();
        var visited = new bool[this.dim, this.dim];

        foreach (var point in area)
        {
            var visitedCopy = (bool[,])visited.Clone();
            var path = this.FindShortestPathToOpenArea(grid, visitedCopy, area, point);

            if (path.Count > 0)
            {
                foreach (var p in path[..^1])
                {
                    visited[p.X, p.Y] = this.random.Next(4) != 0;
                }

                paths.Add(path);
            }
        }

        return paths.GroupBy(p => p.Last()).Select(g => g.First()).ToList();
    }

    private List<Point> FindShortestPathToOpenArea(bool[,] grid, bool[,] visited, List<Point> area, Point start)
    {
        var parent = new Dictionary<Point, Point>();
        var directions = new Point[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        var queue = new Queue<(Point Point, int Distance)>();
        queue.Enqueue((start, 0));
        visited[start.X, start.Y] = true;

        while (queue.Count > 0)
        {
            var (point, distance) = queue.Dequeue();

            if (!grid[point.X, point.Y] && !area.Contains(point))
            {
                var path = new List<Point>();
                var current = point;

                while (current != start)
                {
                    path.Add(current);
                    current = parent[current];
                }

                path.Add(start);
                path.Reverse();

                return path;
            }

            foreach (var dir in directions)
            {
                var next = new Point(point.X + dir.X, point.Y + dir.Y);
                if (next.X >= 0 && next.Y >= 0 && next.X < this.dim && next.Y < this.dim
                    && !visited[next.X, next.Y])
                {
                    visited[next.X, next.Y] = true;
                    parent[next] = point;
                    queue.Enqueue((next, distance + 1));
                }
            }
        }

        return [];
    }

    private List<Point> FloodFill(bool[,] grid, bool[,] visited, int startX, int startY)
    {
        var enclosedArea = new List<Point>();
        var queue = new Queue<Point>();
        queue.Enqueue(new Point(startX, startY));

        var directions = new Point[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        while (queue.Count > 0)
        {
            var point = queue.Dequeue();

            if (point.X < 0 || point.Y < 0 || point.X >= this.dim || point.Y >= this.dim || grid[point.X, point.Y] || visited[point.X, point.Y])
            {
                continue;
            }

            visited[point.X, point.Y] = true;
            enclosedArea.Add(point);

            foreach (var dir in directions)
            {
                queue.Enqueue(new Point(point.X + dir.X, point.Y + dir.Y));
            }
        }

        return enclosedArea;
    }
}
