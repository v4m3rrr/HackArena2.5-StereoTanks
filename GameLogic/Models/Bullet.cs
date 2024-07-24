using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents a bullet.
/// </summary>
public class Bullet : IEquatable<Bullet>
{
    private static int idCounter = 0;

    [JsonProperty]
    private readonly int id = idCounter++;

    private float x;
    private float y;

    /// <summary>
    /// Gets the x coordinate of the bullet.
    /// </summary>
    public int X
    {
        get => (int)this.x;
        init => this.x = value;
    }

    /// <summary>
    /// Gets the y coordinate of the bullet.
    /// </summary>
    public int Y
    {
        get => (int)this.y;
        init => this.y = value;
    }

    /// <summary>
    /// Gets the speed of the bullet.
    /// </summary>
    /// <value>
    /// The speed of the bullet per second.
    /// </value>
    public int Speed { get; init; } = 1;

    /// <summary>
    /// Gets the direction of the bullet.
    /// </summary>
    public Direction Direction { get; init; }

    /// <summary>
    /// Gets the tank that shot the bullet.
    /// </summary>
    public Tank? Shooter { get; init; }

    /// <summary>
    /// Updates the bullet's position based on the current direction, speed and delta time.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void UpdatePosition(float deltaTime)
    {
        float delta = this.Speed * deltaTime;
        switch (this.Direction)
        {
            case Direction.Up:
                this.y -= delta;
                break;
            case Direction.Down:
                this.y += delta;
                break;
            case Direction.Left:
                this.x -= delta;
                break;
            case Direction.Right:
                this.x += delta;
                break;
        }
    }

    /// <summary>
    /// Calculates the trajectory coordinates of the bullet
    /// from the given start coordinates to the current position.
    /// </summary>
    /// <param name="startX">
    /// The starting x coordinate.
    /// </param>
    /// <param name="startY">
    /// The starting y coordinate.
    /// </param>
    /// <returns>
    /// A list of coordinates representing the bullet's trajectory
    /// from the start coordinates to the current position,
    /// excluding the start coordinates.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The start coordinates are not included in the returned coordinates.
    /// </para>
    /// <para>
    /// This method uses the Bresenham's line algorithm
    /// to calculate the coordinates of the bullet's trajectory.
    /// </para>
    /// </remarks>
    public List<(int X, int Y)> CalculateTrajectory(int startX, int startY)
    {
        List<(int X, int Y)> coords = new();
        int dx = Math.Abs(this.X - startX);
        int dy = Math.Abs(this.Y - startY);
        int sx = startX < this.X ? 1 : -1;
        int sy = startY < this.Y ? 1 : -1;
        int err = dx - dy;

        while (startX != this.X || startY != this.Y)
        {
            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                startX += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                startY += sy;
            }

            coords.Add(new(startX, startY));
        }

        return coords;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is equal to the current object;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Bullet);
    }

    /// <inheritdoc cref="Equals(object)"/>
    /// <remarks>
    /// The objects are considered equal if they have the same id.
    /// </remarks>
    public bool Equals(Bullet? other)
    {
        return this.id == other?.id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    /// <remarks>
    /// The hash code is based on the bullet's id.
    /// </remarks>
    public override int GetHashCode()
    {
        return this.id;
    }
}
