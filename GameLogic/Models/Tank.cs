namespace GameLogic;

#if STEREO
/// <summary>
/// Represents a base class for a tank.
/// </summary>
public abstract class Tank
#else
/// <summary>
/// Represents a tank.
/// </summary>
public class Tank
#endif
     : IRegenerable, IEquatable<Tank>
{
    /// <summary>
    /// The number of ticks required for the tank to regenerate.
    /// </summary>
    public const int RegenerationTicks = 50;

    /// <summary>
    /// The maximum health of the tank.
    /// </summary>
    public const int HealthMax = 100;

    private int? remainingRegenerationTicks;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="direction">The tank direction.</param>
    /// <param name="owner">The owner of the tank.</param>
    public Tank(int x, int y, Direction direction, Player owner)
    {
        this.X = x;
        this.Y = y;
        this.Direction = direction;
        this.Owner = owner;
        this.OwnerId = owner.Id;
    }

    /// <summary>
    /// Occurs when the tank regenerates.
    /// </summary>
    internal event EventHandler? Regenerated;

    /// <summary>
    /// Occurs when the tank is about to die.
    /// </summary>
    internal event EventHandler? Dying;

    /// <summary>
    /// Occurs when the tank dies.
    /// </summary>
    internal event EventHandler? Died;

    /// <summary>
    /// Gets the x coordinate of the tank.
    /// </summary>
    public int X { get; private protected set; }

    /// <summary>
    /// Gets the y coordinate of the tank.
    /// </summary>
    public int Y { get; private protected set; }

    /// <summary>
    /// Gets the direction of the tank.
    /// </summary>
    public Direction Direction { get; internal set; }

    /// <summary>
    /// Gets the health of the tank.
    /// </summary>
    public int? Health { get; internal set; } = HealthMax;

    /// <summary>
    /// Gets a value indicating whether the tank is dead.
    /// </summary>
    public bool IsDead => this.Health <= 0;

    /// <inheritdoc/>
    public int TotalRegenerationTicks => RegenerationTicks;

    /// <inheritdoc/>
    public int? RemainingRegenerationTicks
    {
        get => this.remainingRegenerationTicks;
        init
        {
            if (value is not null and < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be greater than or equal to 0.");
            }

            this.remainingRegenerationTicks = value;
        }
    }

    /// <inheritdoc/>
    public float? RegenerationProgress => RegenerationUtils.GetRegenerationProgres(this);

    /// <summary>
    /// Gets or sets the visibility grid of the player.
    /// </summary>
    public bool[,]? VisibilityGrid { get; set; }

    /// <summary>
    /// Gets the owner of the tank.
    /// </summary>
    public Player Owner { get; internal set; }

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    public Turret Turret { get; internal set; } = default!;

#if !STEREO

    /// <summary>
    /// Gets or sets the secondary item of the tank.
    /// </summary>
    public SecondaryItemType? SecondaryItemType { get; set; }

#endif

#if STEREO

    /// <summary>
    /// Gets the type of the tank.
    /// </summary>
    public abstract TankType Type { get; }

#endif

#if !STEREO

    /// <summary>
    /// Gets or sets the radar ability of the tank.
    /// </summary>
    internal RadarAbility? Radar { get; set; }

    /// <summary>
    /// Gets or sets the bullet ability of the tank.
    /// </summary>
    internal MineAbility? Mine { get; set; }

#endif

    /// <summary>
    /// Gets the owner ID of the tank.
    /// </summary>
    internal string OwnerId { get; private protected set; }

    /// <summary>
    /// Gets the previous x coordinate of the tank.
    /// </summary>
    internal int? PreviousX { get; private protected set; }

    /// <summary>
    /// Gets the previous y coordinate of the tank.
    /// </summary>
    internal int? PreviousY { get; private protected set; }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Tank);
    }

    /// <inheritdoc cref="Equals(object)"/>
    public bool Equals(Tank? other)
    {
        return this.OwnerId == other?.OwnerId;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.Owner.GetHashCode();
    }

    /// <inheritdoc/>
    public void RegenerateTick()
    {
        if (!this.IsDead)
        {
            return;
        }

        RegenerationUtils.UpdateRegenerationProcess(ref this.remainingRegenerationTicks);

        if (this.remainingRegenerationTicks is null)
        {
            this.Health = 100;
            this.Regenerated?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc/>
    public void RegenerateFull()
    {
        if (!this.IsDead)
        {
            return;
        }

        this.remainingRegenerationTicks = null;
        this.Health = 100;
        this.Regenerated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Returns all abilities owned by this tank.
    /// </summary>
    /// <returns>
    /// A collection of active abilities.
    /// </returns>
    internal virtual IEnumerable<IAbility> GetAbilities()
    {
#if STEREO
        return [];
#else
        if (this.Radar is not null)
        {
            yield return this.Radar;
        }

        if (this.Mine is not null)
        {
            yield return this.Mine;
        }
#endif
    }

    /// <summary>
    /// Returns the first ability of the specified type owned by this tank.
    /// </summary>
    /// <typeparam name="TAbility">The type of the ability to retrieve.</typeparam>
    /// <returns>The first ability of the specified type, or null if not found.</returns>
    internal virtual TAbility? GetAbility<TAbility>()
    {
        return this.GetAbilities().OfType<TAbility>().FirstOrDefault();
    }

    /// <summary>
    /// Sets the position of the tank.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    internal virtual void SetPosition(int x, int y)
    {
        this.PreviousX = this.X;
        this.PreviousY = this.Y;
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Updates the tank from another tank.
    /// </summary>
    /// <param name="snapshot">The tank to update from.</param>
    internal virtual void UpdateFrom(Tank snapshot)
    {
        this.PreviousX = snapshot.PreviousX;
        this.PreviousY = snapshot.PreviousY;
        this.X = snapshot.X;
        this.Y = snapshot.Y;
        this.Direction = snapshot.Direction;
        this.Health = snapshot.Health;
        this.VisibilityGrid = snapshot.VisibilityGrid;
        this.Turret.UpdateFrom(snapshot.Turret);

#if !STEREO
        this.SecondaryItemType = snapshot.SecondaryItemType;
        this.Radar?.UpdateFrom(snapshot.Radar!);
        this.Mine?.UpdateFrom(snapshot.Mine!);
#endif
    }

    /// <summary>
    /// Invokes the <see cref="Dying"/> event.
    /// </summary>
    /// <param name="e">The event arguments to pass to the event handler.</param>
    internal void OnDying(EventArgs e)
    {
        this.Dying?.Invoke(this, e);
    }

    /// <summary>
    /// Invokes the <see cref="Dying"/> event.
    /// </summary>
    /// <param name="e">The event arguments to pass to the event handler.</param>
    internal void OnDied(EventArgs e)
    {
        this.remainingRegenerationTicks = RegenerationTicks;
        this.Died?.Invoke(this, e);
    }
}
