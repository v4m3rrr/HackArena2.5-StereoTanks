namespace GameLogic;

/// <summary>
/// Represents a mine.
/// </summary>
public class Mine : IStunEffect, IEquatable<Mine>
{
    /// <summary>
    /// The number of ticks that the explosion lasts.
    /// </summary>
    public const int ExplosionTicks = 10;

    private static int idCounter = 0;

    private int? explosionRemainingTicks = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mine"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the mine.</param>
    /// <param name="y">The y coordinate of the mine.</param>
    /// <param name="damage">The damage dealt by the mine.</param>
    /// <param name="layer">The player that deployed the mine.</param>
    /// <remarks>
    /// <para>This constructor should be used when a tank deploys a mine.</para>
    /// <para>The <see cref="Id"/> property is set automatically.</para>
    /// </remarks>
    internal Mine(int x, int y, int damage, Player layer)
        : this(idCounter++, x, y, damage, layer.Id)
    {
        this.Layer = layer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mine"/> class.
    /// </summary>
    /// <param name="id">The id of the mine.</param>
    /// <param name="x">The x coordinate of the mine.</param>
    /// <param name="y">The y coordinate of the mine.</param>
    /// <param name="damage">The damage dealt by the mine.</param>
    /// <param name="layerId">The id of the player that dropped the mine.</param>
    internal Mine(int id, int x, int y, int? damage = null, string? layerId = null)
    {
        this.Id = id;
        this.X = x;
        this.Y = y;
        this.Damage = damage;
        this.LayerId = layerId;
    }

    /// <inheritdoc/>
    int IStunEffect.StunTicks => 10;

    /// <inheritdoc/>
    StunBlockEffect IStunEffect.StunBlockEffect
        => StunBlockEffect.Movement | StunBlockEffect.TankRotation;

    /// <summary>
    /// Gets the id of the mine.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the x coordinate of the mine.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    /// Gets the y coordinate of the mine.
    /// </summary>
    public int Y { get; private set; }

    /// <summary>
    /// Gets the damage dealt by the mine.
    /// </summary>
    public int? Damage { get; private set; }

    /// <summary>
    /// Gets the id of the player that deployed the mine.
    /// </summary>
    public string? LayerId { get; private set; }

    /// <summary>
    /// Gets the player that deployed the mine.
    /// </summary>
    public Player? Layer { get; internal set; }

    /// <summary>
    /// Gets the remaining ticks of the mine's explosion.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/>
    /// if the mine hasn't exploded yet.
    /// </remarks>
    public int? ExplosionRemainingTicks
    {
        get => this.explosionRemainingTicks;
        init => this.explosionRemainingTicks = value;
    }

    /// <summary>
    /// Gets a value indicating whether the mine is exploded.
    /// </summary>
    public bool IsExploded => this.ExplosionRemainingTicks is not null;

    /// <summary>
    /// Gets a value indicating whether the mine is fully exploded.
    /// </summary>
    /// <remarks>
    /// The mine is fully exploded if it has exploded
    /// and the explosion has finished.
    /// </remarks>
    public bool IsFullyExploded => this.ExplosionRemainingTicks <= 0;

#if STEREO && SERVER

    /// <summary>
    /// Gets or sets a value indicating whether the mine should explode next tick.
    /// </summary>
    public bool ShouldExplodeNextTick { get; set; }

#endif

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Mine);
    }

    /// <inheritdoc/>
    public bool Equals(Mine? other)
    {
        return this.Id == other?.Id;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.Id;
    }

    /// <summary>
    /// Explodes the mine.
    /// </summary>
    public void Explode()
    {
        this.explosionRemainingTicks = ExplosionTicks;
    }

    /// <summary>
    /// Decreases the explosion ticks,
    /// if the mine has exploded.
    /// </summary>
    public void DecreaseExplosionTicks()
    {
        if (this.explosionRemainingTicks is not null)
        {
            this.explosionRemainingTicks--;
        }
    }

    /// <summary>
    /// Updates the mine with the values from another mine.
    /// </summary>
    /// <param name="snapshot">The mine to copy the values from.</param>
    public void UpdateFrom(Mine snapshot)
    {
        this.X = snapshot.X;
        this.Y = snapshot.Y;
        this.Damage = snapshot.Damage;
        this.LayerId = snapshot.LayerId;
        this.explosionRemainingTicks = snapshot.explosionRemainingTicks;
    }
}
