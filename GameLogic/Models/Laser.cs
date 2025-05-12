namespace GameLogic;

/// <summary>
/// Represents a laser.
/// </summary>
public class Laser : IStunEffect, IEquatable<Laser>
{
    private const int BlastTicks = 10;

    private static int idCounter = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Laser"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the laser.</param>
    /// <param name="y">The y coordinate of the laser.</param>
    /// <param name="orientation">The orientation of the laser.</param>
    /// <param name="damage">The damage dealt by the laser.</param>
    /// <param name="shooter">The tank that shot the laser.</param>
    /// <remarks>
    /// <para>This constructor should be used when a tank uses a laser.</para>
    /// <para>The <see cref="Id"/> property is set automatically.</para>
    /// </remarks>
    internal Laser(int x, int y, Orientation orientation, int damage, Player shooter)
        : this(idCounter++, x, y, orientation, damage, shooter.Id)
    {
        this.Shooter = shooter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Laser"/> class.
    /// </summary>
    /// <param name="id">The id of the laser.</param>
    /// <param name="x">The x coordinate of the laser.</param>
    /// <param name="y">The y coordinate of the laser.</param>
    /// <param name="orientation">The orientation of the laser.</param>
    /// <param name="damage">The damage dealt by the laser.</param>
    /// <param name="shooterId">The id of the tank that used the laser.</param>
    internal Laser(
        int id,
        int x,
        int y,
        Orientation orientation,
        int? damage = null,
        string? shooterId = null)
    {
        this.Id = id;
        this.X = x;
        this.Y = y;
        this.Orientation = orientation;
        this.Damage = damage;
        this.ShooterId = shooterId;
    }

    /// <inheritdoc/>
    int IStunEffect.StunTicks => BlastTicks;

    /// <inheritdoc/>
    StunBlockEffect IStunEffect.StunBlockEffect => StunBlockEffect.All;

    /// <summary>
    /// Gets the id of the laser.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the x coordinate of the laser.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    /// Gets the y coordinate of the laser.
    /// </summary>
    public int Y { get; private set; }

    /// <summary>
    /// Gets the orientation of the laser.
    /// </summary>
    public Orientation Orientation { get; private set; }

    /// <summary>
    /// Gets the damage dealt by the laser.
    /// </summary>
    /// <value>
    /// The damage dealt by the laser per tick.
    /// </value>
    public int? Damage { get; private set; }

    /// <summary>
    /// Gets the remaining ticks of the laser.
    /// </summary>
    internal int RemainingTicks { get; private set; } = BlastTicks;

    /// <summary>
    /// Gets the id of the player that used the laser.
    /// </summary>
    internal string? ShooterId { get; private set; }

    /// <summary>
    /// Gets or sets the tank that used the laser.
    /// </summary>
    internal Player? Shooter { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Laser);
    }

    /// <inheritdoc/>
    public bool Equals(Laser? other)
    {
        return this.Id == other?.Id;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.Id;
    }

    /// <summary>
    /// Decreases the remaining ticks of the laser.
    /// </summary>
    public void DecreaseRemainingTicks()
    {
        this.RemainingTicks--;
    }

    /// <summary>
    /// Updates the laser with the values of another laser.
    /// </summary>
    /// <param name="snapshot">The laser to copy the values from.</param>
    public void UpdateFrom(Laser snapshot)
    {
        this.Orientation = snapshot.Orientation;
        this.Damage = snapshot.Damage;
        this.ShooterId = snapshot.ShooterId;
    }
}
