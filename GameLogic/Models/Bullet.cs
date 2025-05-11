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
        : this(idCounter++, x, y, direction, speed, damage, shooter.Id)
    {
        this.Shooter = shooter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Bullet"/> class.
    /// </summary>
    /// <param name="id">The id of the bullet.</param>
    /// <param name="x">The x coordinate of the bullet.</param>
    /// <param name="y">The y coordinate of the bullet.</param>
    /// <param name="direction">The direction of the bullet.</param>
    /// <param name="speed">The speed of the bullet per second.</param>
    /// <param name="damage">The damage dealt by the bullet.</param>
    /// <param name="shooterId">The id of the player that shot the bullet.</param>
    internal Bullet(
        int id,
        int x,
        int y,
        Direction direction,
        float speed,
        int? damage = null,
        string? shooterId = null)
    {
        this.Id = id;
        this.x = x;
        this.y = y;
        this.Direction = direction;
        this.Speed = speed;
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
    public Direction Direction { get; private set; }

    /// <summary>
    /// Gets the speed of the bullet.
    /// </summary>
    /// <value>
    /// The speed of the bullet per second.
    /// </value>
    public float Speed { get; private set; }

    /// <summary>
    /// Gets the damage dealt by the bullet.
    /// </summary>
    public int? Damage { get; private set; }

    /// <summary>
    /// Gets the id of the owner of the bullet.
    /// </summary>
    internal string? ShooterId { get; private set; }

    /// <summary>
    /// Gets or sets the tank that shot the bullet.
    /// </summary>
    internal Player? Shooter { get; set; }

    /// <summary>
    /// Gets the type of the bullet.
    /// </summary>
    internal virtual BulletType Type => BulletType.Basic;

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

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Bullet);
    }

    /// <inheritdoc/>
    public bool Equals(Bullet? other)
    {
        return this.Id == other?.Id;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.Id;
    }

    /// <summary>
    /// Updates the bullet's properties from another bullet.
    /// </summary>
    /// <param name="snapshot">The bullet to update from.</param>
    public void UpdateFrom(Bullet snapshot)
    {
        this.x = snapshot.X;
        this.y = snapshot.Y;
        this.Direction = snapshot.Direction;
    }
}
