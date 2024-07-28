using System.Drawing;

namespace GameLogic;

/// <summary>
/// Represents a map generator.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MapGenerator"/> class.
/// </remarks>
/// <param name="seed">The seed for the random number generator.</param>
internal class MapGenerator(int seed)
{
    private const int Dim = Grid.Dim;
    private const int InnerWalls = Grid.InnerWalls;

    private readonly Random random = new(seed);

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
        var grid = new bool[Dim, Dim];

        for (int i = 0; i < Dim; i++)
        {
            for (int j = 0; j < Dim; j++)
            {
                grid[i, j] = i == 0 || j == 0 || i == Dim - 1 || j == Dim - 1;
            }
        }

        int maxInnerWalls = Math.Min((Dim - 2) * (Dim - 2) * 8 / 10, InnerWalls);
        int innerWalls = 0;

        while (innerWalls < maxInnerWalls)
        {
            int x = this.random.Next(1, Dim - 1);
            int y = this.random.Next(1, Dim - 1);

            grid[x, y] = true;
            innerWalls++;
        }

        this.ConnectClosedSpaces(grid);

        return grid;
    }

    private void ConnectClosedSpaces(bool[,] grid)
    {
        var visited = new bool[Dim, Dim];

        for (int i = 1; i < Dim - 1; i++)
        {
            for (int j = 1; j < Dim - 1; j++)
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

            if (point.X < 0 || point.Y < 0 || point.X >= Dim || point.Y >= Dim || grid[point.X, point.Y] || visited[point.X, point.Y])
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

                if (newX > 0 && newY > 0 && newX < Dim - 1 && newY < Dim - 1 && grid[newX, newY])
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
