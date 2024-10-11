namespace GameLogic;

/// <summary>
/// Represents a bullet.
/// </summary>
public class Bullet : IEquatable<Bullet>
{
    private static int idCounter = 0;

    private float x;
    private float y;

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the bullet.</param>
    /// <param name="y">The y coordinate of the bullet.</param>
    /// <param name="direction">The direction of the bullet.</param>
    /// <param name="speed">The speed of the bullet per second.</param>
    /// <param name="damage">The damage dealt by the bullet.</param>
    /// <param name="shooter">The tank that shot the bullet.</param>
    /// <remarks>
    /// <para>This constructor should be used when a tank shoots a bullet.</para>
    /// <para>The <see cref="Id"/> property is set automatically.</para>
    /// </remarks>
    internal Bullet(int x, int y, Direction direction, float speed, int damage, Player shooter)
        : this(idCounter++, x, y, direction, speed)
    {
        this.Damage = damage;
        this.Shooter = shooter;
        this.ShooterId = shooter.Id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="id">The id of the bullet.</param>
    /// <param name="x">The x coordinate of the bullet.</param>
    /// <param name="y">The y coordinate of the bullet.</param>
    /// <param name="direction">The direction of the bullet.</param>
    /// <param name="speed">The speed of the bullet per second.</param>
    /// <remarks>
    /// This constructor should be used when creating a bullet
    /// from player perspective, because they shouldn't know
    /// the <see cref="ShooterId"/>, <see cref="Shooter"/>
    /// and <see cref="Damage"/> (these will be set to <see langword="null"/>).
    /// </remarks>
    internal Bullet(int id, int x, int y, Direction direction, float speed)
    {
        this.Id = id;
        this.x = x;
        this.y = y;
        this.Direction = direction;
        this.Speed = speed;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="id">The id of the bullet.</param>
    /// <param name="direction">The direction of the bullet.</param>
    /// <param name="speed">The speed of the bullet per second.</param>
    /// <param name="x">The x coordinate of the bullet.</param>
    /// <param name="y">The y coordinate of the bullet.</param>
    /// <param name="damage">The damage dealt by the bullet.</param>
    /// <param name="shooterId">The id of the tank that shot the bullet.</param>
    /// <remarks>
    /// <para>
    /// This constructor should be used when creating a bullet
    /// from the server or spectator perspective, because they know
    /// all the properties of the bullet.
    /// </para>
    /// <para>
    /// This constructor does not set the <see cref="Shooter"/> property.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    internal Bullet(int id, int x, int y, Direction direction, float speed, int damage, string shooterId)
        : this(id, x, y, direction, speed)
    {
        this.Damage = damage;
        this.ShooterId = shooterId;
    }

    /// <summary>
    /// Gets the id of the bullet.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the x coordinate of the bullet.
    /// </summary>
    public int X => (int)this.x;

    /// <summary>
    /// Gets the y coordinate of the bullet.
    /// </summary>
    public int Y => (int)this.y;

    /// <summary>
    /// Gets the direction of the bullet.
    /// </summary>
    public Direction Direction { get; }

    /// <summary>
    /// Gets the speed of the bullet.
    /// </summary>
    /// <value>
    /// The speed of the bullet per second.
    /// </value>
    public float Speed { get; }

    /// <summary>
    /// Gets the damage dealt by the bullet.
    /// </summary>
    public int? Damage { get; }

    /// <summary>
    /// Gets the id of the owner of the bullet.
    /// </summary>
    internal string? ShooterId { get; }

    /// <summary>
    /// Gets or sets the tank that shot the bullet.
    /// </summary>
    /// <remarks>
    /// This value should be set in the
    /// <see cref="Grid.UpdateFromGameStatePayload"/>
    /// method, if the <see cref="ShooterId"/> is known.
    /// </remarks>
    internal Player? Shooter { get; set; }

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

        if (startX == this.X && startY == this.Y)
        {
            coords.Add(new(startX, startY));
        }

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
        return this.Id == other?.Id;
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
        return this.Id;
    }
}
