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
    /// <returns>
    /// The 2D array representing the wall grid,
    /// where <c>true</c> means a wall
    /// and <c>false</c> means an empty space.
    /// </returns>
    public bool[,] GenerateWalls()
    {
        var grid = new bool[this.dim, this.dim];

        int maxInnerWalls = this.dim * this.dim * 8 / 10;
        int innerWalls = 0;

        while (innerWalls < maxInnerWalls)
        {
            int x = this.random.Next(0, this.dim);
            int y = this.random.Next(0, this.dim);

            grid[x, y] = true;
            innerWalls++;
        }

        this.ConnectClosedSpaces(grid);

        return grid;
    }

    /// <summary>
    /// Generates a list of zones.
    /// </summary>
    /// <returns>The list of zones.</returns>
    public List<Zone> GenerateZones()
    {
        const int height = 4;
        const int width = 4;
        const int count = 2;
        const int maxAttempts = 10000;

        var zones = new List<Zone>();

        bool IsOverlapping(int x, int y) => zones.Any(
            z => x < z.X + z.Width + width + 1
                && x + width + 1 > z.X
                && y < z.Y + z.Height + height + 1
                && y + height + 1 > z.Y);

        for (int i = 0; i < count; i++)
        {
            int x, y;
            int attempts = 0;

            do
            {
                x = this.random.Next(1, this.dim - width - 2);
                y = this.random.Next(1, this.dim - height - 2);

                if (attempts++ >= maxAttempts)
                {
                    this.GenerationWarning?.Invoke(
                        this,
                        "Max attempts of generating zones reached. Generated: {zones.Count} from {count}");

                    return zones;
                }
            } while (IsOverlapping(x, y));

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

    /// <summary>
    /// Removes some walls from the specified zones.
    /// </summary>
    /// <param name="grid">The 2D array representing the wall grid.</param>
    /// <param name="zones">The zones from which to remove walls.</param>
    /// <remarks>
    /// The number of walls to remove is randomly chosen
    /// between 10% and 20% of the total walls in the zone,
    /// or less if there are fewer walls.
    /// </remarks>
    public void RemoveSomeWallsFromZones(bool[,] grid, IEnumerable<Zone> zones)
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

    private void ConnectClosedSpaces(bool[,] grid)
    {
        var visited = new bool[this.dim, this.dim];

        for (int i = 0; i < this.dim; i++)
        {
            for (int j = 0; j < this.dim; j++)
            {
                if (!visited[i, j] && !grid[i, j])
                {
                    var closedSpace = this.FloodFill(grid, visited, i, j);
                    this.RemoveWallToOpenSpace(grid, closedSpace);
                }
            }
        }
    }

    private List<Point> FloodFill(bool[,] grid, bool[,] visited, int startX, int startY)
    {
        var space = new List<Point>();
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
            space.Add(point);

            foreach (var dir in directions)
            {
                queue.Enqueue(new Point(point.X + dir.X, point.Y + dir.Y));
            }
        }

        return space;
    }

    private void RemoveWallToOpenSpace(bool[,] grid, List<Point> space)
    {
        var directions = new Point[] { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        var wallsRemoved = 0;
        var maxWallsToRemove = Math.Min(2, space.Count);
        var attemptedRemovals = 0;
        var maxAttempts = space.Count * directions.Length * 10;

        while (wallsRemoved < maxWallsToRemove && attemptedRemovals < maxAttempts)
        {
            var cell = space[this.random.Next(space.Count)];
            foreach (var dir in directions)
            {
                int newX = cell.X + dir.X;
                int newY = cell.Y + dir.Y;

                if (newX >= 0 && newY >= 0 && newX < this.dim && newY < this.dim && grid[newX, newY])
                {
                    grid[newX, newY] = false;
                    wallsRemoved++;
                    break;
                }
            }

            attemptedRemovals++;
        }
    }
}
