using System.Numerics;

namespace GameLogic;

/// <summary>
/// Represents the fog of war manager.
/// </summary>
/// <param name="wallGrid">
/// The grid representing the walls.
/// A cell is considered a wall if the value is <c>true</c>.
/// </param>
internal class FogOfWarManager(bool[,] wallGrid)
{
    private readonly bool[,] wallGrid = wallGrid;
    private readonly int width = wallGrid.GetLength(0);
    private readonly int height = wallGrid.GetLength(1);

    /// <summary>
    /// Gets an empty grid.
    /// </summary>
    public bool[,] EmptyGrid => new bool[this.width, this.height];

    /// <summary>
    /// Determines whether the specified element is visible.
    /// </summary>
    /// <param name="visibilityGrid">The visibility grid.</param>
    /// <param name="x">The x-coordinate of the element.</param>
    /// <param name="y">The y-coordinate of the element.</param>
    /// <returns>
    /// <see langword="true"/> if the element is visible;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method also checks if the element
    /// is within the bounds of the visibility grid.
    /// </remarks>
    public static bool IsElementVisible(bool[,] visibilityGrid, int x, int y)
    {
        return x >= 0 && x < visibilityGrid.GetLength(0)
            && y >= 0 && y < visibilityGrid.GetLength(1)
            && visibilityGrid[x, y];
    }

    /// <summary>
    /// Calculates the visibility grid for the specified tank.
    /// </summary>
    /// <param name="tank">The tank for which to calculate the visibility grid.</param>
    /// <param name="viewAngle">The view angle of the tank in degrees.</param>
    /// <returns>The visibility grid for the specified tank.</returns>
    public bool[,] CalculateVisibilityGrid(Tank tank, int viewAngle)
    {
        var visibilityGrid = new bool[this.width, this.height];
        var tankPosition = new Vector2(tank.X + 0.5f, tank.Y + 0.5f);
        var tankDirection = DirectionUtils.ToDegrees(tank.Direction);
        var visited = new bool[this.width, this.height];
        var queue = new Queue<(int X, int Y)>();

        queue.Enqueue((tank.X, tank.Y));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            if (visited[x, y] || this.wallGrid[x, y])
            {
                continue;
            }

            visited[x, y] = true;

            bool isVisible = this.IsCellVisible(tankPosition, new(x + 0.25f, y + 0.25f), viewAngle, tankDirection)
                || this.IsCellVisible(tankPosition, new(x + 0.25f, y + 0.75f), viewAngle, tankDirection)
                || this.IsCellVisible(tankPosition, new(x + 0.75f, y + 0.75f), viewAngle, tankDirection)
                || this.IsCellVisible(tankPosition, new(x + 0.75f, y + 0.25f), viewAngle, tankDirection);

            if (isVisible)
            {
                visibilityGrid[x, y] = true;
                this.EnqueueAdjacentCells(queue, x, y);
            }
        }

        this.UpdateTurretVisibility(tank, visibilityGrid);

        return visibilityGrid;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle < -180)
        {
            angle += 360;
        }

        while (angle > 180)
        {
            angle -= 360;
        }

        return angle;
    }

    private void UpdateTurretVisibility(Tank tank, bool[,] visibilityGrid)
    {
        int startX = tank.X;
        int startY = tank.Y;
        int endX = tank.X;
        int endY = tank.Y;
        int stepX = 0;
        int stepY = 0;

        switch (tank.Turret.Direction)
        {
            case Direction.Up:
                startY = tank.Y;
                endY = -1;
                stepY = -1;
                break;
            case Direction.Right:
                startX = tank.X;
                endX = this.width;
                stepX = 1;
                break;
            case Direction.Down:
                startY = tank.Y;
                endY = this.height;
                stepY = 1;
                break;
            case Direction.Left:
                startX = tank.X;
                endX = -1;
                stepX = -1;
                break;
        }

        int x = startX;
        int y = startY;

        while (x != endX || y != endY)
        {
            if (this.wallGrid[x, y])
            {
                break;
            }

            visibilityGrid[x, y] = true;

            x += stepX;
            y += stepY;
        }
    }

    private bool IsCellVisible(Vector2 tankPosition, Vector2 cellPosition, float viewAngle, float tankDirection)
    {
        float dx = cellPosition.X - tankPosition.X;
        float dy = cellPosition.Y - tankPosition.Y;

        float angleToCell = (MathF.Atan2(dy, dx) * 180f / MathF.PI) + 90f;
        float angleDifference = NormalizeAngle(angleToCell - tankDirection);

        return Math.Abs(angleDifference) <= viewAngle / 2
            && this.IsLineOfSightClear(tankPosition, cellPosition);
    }

    private bool IsLineOfSightClear(Vector2 start, Vector2 end)
    {
        float x0 = start.X;
        float y0 = start.Y;
        float x1 = end.X;
        float y1 = end.Y;

        float dx = Math.Abs(x1 - x0);
        float dy = Math.Abs(y1 - y0);
        float sx = x0 < x1 ? 1 : -1;
        float sy = y0 < y1 ? 1 : -1;
        float err = dx - dy;

        const float threshold = 0.1f;
        const float increment = 0.1f;

        while (true)
        {
            int ix = (int)Math.Floor(x0);
            int iy = (int)Math.Floor(y0);

            if (this.wallGrid[ix, iy])
            {
                return false;
            }

            if ((Math.Abs(x0 - x1) < threshold) && (Math.Abs(y0 - y1) < threshold))
            {
                break;
            }

            float e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx * increment;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy * increment;
            }
        }

        return true;
    }

    private void EnqueueAdjacentCells(Queue<(int X, int Y)> queue, int x, int y)
    {
        if (x > 0)
        {
            queue.Enqueue((x - 1, y));
        }

        if (x < this.width - 1)
        {
            queue.Enqueue((x + 1, y));
        }

        if (y > 0)
        {
            queue.Enqueue((x, y - 1));
        }

        if (y < this.height - 1)
        {
            queue.Enqueue((x, y + 1));
        }
    }
}
