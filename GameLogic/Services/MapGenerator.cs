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

        for (int i = 0; i < this.dim; i++)
        {
            for (int j = 0; j < this.dim; j++)
            {
                grid[i, j] = i == 0 || j == 0 || i == this.dim - 1 || j == this.dim - 1;
            }
        }

        int maxInnerWalls = (this.dim - 2) * (this.dim - 2) * 8 / 10;
        int innerWalls = 0;

        while (innerWalls < maxInnerWalls)
        {
            int x = this.random.Next(1, this.dim - 1);
            int y = this.random.Next(1, this.dim - 1);

            grid[x, y] = true;
            innerWalls++;
        }

        this.ConnectClosedSpaces(grid);

        return grid;
    }

    private void ConnectClosedSpaces(bool[,] grid)
    {
        var visited = new bool[this.dim, this.dim];

        for (int i = 1; i < this.dim - 1; i++)
        {
            for (int j = 1; j < this.dim - 1; j++)
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

                if (newX > 0 && newY > 0 && newX < this.dim - 1 && newY < this.dim - 1 && grid[newX, newY])
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
