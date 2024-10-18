namespace GameLogic;

/// <summary>
/// Represents a laser.
/// </summary>
public class Laser : IStunEffect, IEquatable<Laser>
{
    private const int BlastTicks = 10;

    private static int idCounter = 0;

    private readonly float x;
    private readonly float y;

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
        : this(idCounter++, x, y, orientation)
    {
        this.Damage = damage;
        this.Shooter = shooter;
        this.ShooterId = shooter.Id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Laser"/> class.
    /// </summary>
    /// <param name="id">The id of the laser.</param>
    /// <param name="x">The x coordinate of the laser.</param>
    /// <param name="y">The y coordinate of the laser.</param>
    /// <param name="orientation">The orientation of the laser.</param>
    /// <remarks>
    /// This constructor should be used when creating a laser
    /// from player perspective, because they shouldn't know
    /// the <see cref="ShooterId"/>, <see cref="Shooter"/>
    /// and <see cref="Damage"/> (these will be set to <see langword="null"/>).
    /// </remarks>
    internal Laser(int id, int x, int y, Orientation orientation)
    {
        this.Id = id;
        this.x = x;
        this.y = y;
        this.Orientation = orientation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Laser"/> class.
    /// </summary>
    /// <param name="id">The id of the laser.</param>
    /// <param name="orientation">The orientation of the laser.</param>
    /// <param name="x">The x coordinate of the laser.</param>
    /// <param name="y">The y coordinate of the laser.</param>
    /// <param name="damage">The damage dealt by the laser.</param>
    /// <param name="shooterId">The id of the tank that used the laser.</param>
    /// <remarks>
    /// <para>
    /// This constructor should be used when creating a laser
    /// from the server or spectator perspective, because they know
    /// all the properties of the laser.
    /// </para>
    /// <para>
    /// This constructor does not set the <see cref="Shooter"/> property.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    internal Laser(int id, int x, int y, Orientation orientation, int damage, string shooterId)
        : this(id, x, y, orientation)
    {
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
    public int X => (int)this.x;

    /// <summary>
    /// Gets the y coordinate of the laser.
    /// </summary>
    public int Y => (int)this.y;

    /// <summary>
    /// Gets the orientation of the laser.
    /// </summary>
    public Orientation Orientation { get; }

    /// <summary>
    /// Gets the damage dealt by the laser.
    /// </summary>
    /// <value>
    /// The damage dealt by the laser per tick.
    /// </value>
    public int? Damage { get; }

    /// <summary>
    /// Gets the remaining ticks of the laser.
    /// </summary>
    public int RemainingTicks { get; private set; } = BlastTicks;

    /// <summary>
    /// Gets the id of the player that used the laser.
    /// </summary>
    internal string? ShooterId { get; }

    /// <summary>
    /// Gets or sets the tank that used the laser.
    /// </summary>
    /// <remarks>
    /// This value should be set in the
    /// <see cref="Grid.UpdateFromGameStatePayload"/>
    /// method, if the <see cref="ShooterId"/> is known.
    /// </remarks>
    internal Player? Shooter { get; set; }

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
        return this.Equals(obj as Laser);
    }

    /// <inheritdoc cref="Equals(object)"/>
    /// <remarks>
    /// The objects are considered equal if they have the same id.
    /// </remarks>
    public bool Equals(Laser? other)
    {
        return this.Id == other?.Id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    /// <remarks>
    /// The hash code is based on the laser's id.
    /// </remarks>
    public override int GetHashCode()
    {
        return this.Id;
    }

    /// <summary>
    /// Decreases the remaining ticks of the laser.
    /// </summary>
    internal void DecreaseRemainingTicks()
    {
        this.RemainingTicks--;
    }
}
