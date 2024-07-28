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
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="shooter">The tank that shot the bullet.</param>
    internal Bullet(Player shooter)
    {
        this.Shooter = shooter;
        this.ShooterId = shooter.Id;
    }

    [JsonConstructor]
    private Bullet()
    {
    }

    /// <summary>
    /// Gets the damage dealt by the bullet.
    /// </summary>
    public int Damage { get; private init; } = 20;

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
    [JsonProperty]
    public int Speed { get; internal init; } = 1;

    /// <summary>
    /// Gets the direction of the bullet.
    /// </summary>
    [JsonProperty]
    public Direction Direction { get; internal init; }

    /// <summary>
    /// Gets the id of the owner of the bullet.
    /// </summary>
    [JsonProperty]
    public string ShooterId { get; private init; } = default!;

    /// <summary>
    /// Gets the tank that shot the bullet.
    /// </summary>
    /// <remarks>
    /// This property has <see cref="JsonIgnoreAttribute"/> because it
    /// is set in the <see cref="Networking.GameStatePayload.GridState"/>
    /// init property when deserializing the game state,
    /// based on the <see cref="ShooterId"/> property.
    /// </remarks>
    [JsonIgnore]
    public Player Shooter { get; internal set; } = default!;

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
        List<(int X, int Y)> coords = [];
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
