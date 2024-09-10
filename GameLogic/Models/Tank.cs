using System.Diagnostics;

namespace GameLogic;

/// <summary>
/// Represents a tank.
/// </summary>
public class Tank : IEquatable<Tank>
{
    private const int RegenTicks = 50;
    private int ticksUntilRegen = RegenTicks;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="owner">The owner of the tank.</param>
    internal Tank(int x, int y, Player owner)
        : this(x, y, owner.Id)
    {
        this.Owner = owner;
        this.Health = 100;
        this.Turret = new Turret(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="ownerId">The owner ID of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turret">The turret of the tank.</param>
    /// <remarks>
    /// <para>
    /// This constructor should be used when creating a tank
    /// from player perspective, because they shouldn't know
    /// the <see cref="Health"/>, <see cref="RegenProgress"/>
    /// (these will be set to <see langword="null"/>).
    /// </para>
    /// <para>
    /// The <see cref="Owner"/> property is set to <see langword="null"/>.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    internal Tank(int x, int y, string ownerId, Direction direction, Turret turret)
        : this(x, y, ownerId)
    {
        this.Direction = direction;
        this.Turret = turret;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="ownerId">The owner ID of the tank.</param>
    /// <param name="health">The health of the tank.</param>
    /// <param name="regenProgress">The regeneration progress of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turret">The turret of the tank.</param>
    /// <remarks>
    /// <para>
    /// This constructor should be used when creating a tank
    /// from the server or spectator perspective, because they know
    /// all the properties of the tank.
    /// </para>
    /// <para>
    /// The <see cref="Owner"/> property is set to <see langword="null"/>.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    internal Tank(int x, int y, string ownerId, int health, float? regenProgress, Direction direction, Turret turret)
        : this(x, y, ownerId, direction, turret)
    {
        this.Health = health;
        this.RegenProgress = regenProgress;
    }

    private Tank(int x, int y, string ownerId)
    {
        this.X = x;
        this.Y = y;
        this.Owner = null!;
        this.OwnerId = ownerId;
        this.Direction = EnumUtils.Random<Direction>();
        this.Turret = new Turret(this);
    }

    /// <summary>
    /// Occurs when the tank dies.
    /// </summary>
    internal event EventHandler? Died;

    /// <summary>
    /// Occurs when the tank regenerates.
    /// </summary>
    internal event EventHandler? Regenerated;

    /// <summary>
    /// Gets the x coordinate of the tank.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    /// Gets the y coordinate of the tank.
    /// </summary>
    public int Y { get; private set; }

    /// <summary>
    /// Gets the health of the tank.
    /// </summary>
    public int? Health { get; private set; }

    /// <summary>
    /// Gets the regeneration progress of the tank.
    /// </summary>
    public float? RegenProgress { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the tank is dead.
    /// </summary>
    public bool IsDead => this.Health <= 0;

    /// <summary>
    /// Gets the owner of the tank.
    /// </summary>
    /// <remarks>
    /// The setter is internal because the owner is set
    /// in the <see cref="Grid.UpdateFromStatePayload"/> method.
    /// </remarks>
    public Player Owner { get; internal set; }

    /// <summary>
    /// Gets the direction of the tank.
    /// </summary>
    public Direction Direction { get; private set; } = EnumUtils.Random<Direction>();

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    public Turret Turret { get; private set; }

    /// <summary>
    /// Gets the owner ID of the tank.
    /// </summary>
    internal string OwnerId { get; private set; }

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
        return this.Equals(obj as Tank);
    }

    /// <inheritdoc cref="Equals(object)"/>
    public bool Equals(Tank? other)
    {
        return this.OwnerId == other?.OwnerId;
    }

    /// <summary>
    /// Gets the hash code of the tank.
    /// </summary>
    /// <returns>The hash code of the tank.</returns>
    public override int GetHashCode()
    {
        return this.Owner.GetHashCode();
    }

    /// <summary>
    /// Rotates the tank.
    /// </summary>
    /// <param name="rotation">The rotation to apply.</param>
    public void Rotate(Rotation rotation)
    {
        this.Direction = rotation switch
        {
            Rotation.Left => EnumUtils.Previous(this.Direction),
            Rotation.Right => EnumUtils.Next(this.Direction),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Creates a tank with only the regeneration progress set.
    /// </summary>
    /// <param name="ownerId">The owner ID of the tank.</param>
    /// <param name="progress">The regeneration progress of the tank.</param>
    /// <returns>
    /// A tank with only the regeneration progress set.
    /// </returns>
    /// <remarks>
    /// <para>
    /// It is used to create a tank with only the regeneration progress set,
    /// for example, when the tank is dead but GUI still needs to display the
    /// regeneration progress of the tank.
    /// </para>
    /// <para>
    /// The position of the tank is set to (-1, -1) and the health is set to -1.
    /// </para>
    /// </remarks>
    internal static Tank OnlyRegenProgress(string ownerId, float progress)
    {
        return new Tank(-1, -1, ownerId)
        {
            Health = -1,
            RegenProgress = progress,
        };
    }

    /// <summary>
    /// Reduces the health of the tank.
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    internal void TakeDamage(int damage)
    {
        Debug.Assert(damage >= 0, "Damage cannot be negative.");

        this.Health -= damage;

        if (this.Health <= 0)
        {
            this.SetPosition(-1, -1);
            this.Died?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Heals the tank by the specified amount of points.
    /// </summary>
    /// <param name="points">The amount of points to heal.</param>
    internal void Heal(int points)
    {
        Debug.Assert(points >= 0, "Healing points cannot be negative.");

        if (this.Health < 100)
        {
            this.Health += points;
        }
    }

    /// <summary>
    /// Sets the position of the tank.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    internal void SetPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Regenerates the tank over time, if it is dead.
    /// </summary>
    internal void Regenerate()
    {
        if (!this.IsDead)
        {
            return;
        }

        if (this.ticksUntilRegen > 0)
        {
            this.RegenProgress = (float)(RegenTicks - --this.ticksUntilRegen) / RegenTicks;
        }

        if (this.ticksUntilRegen <= 0)
        {
            this.Health = 100;
            this.ticksUntilRegen = RegenTicks;
            this.RegenProgress = null;
            this.Regenerated?.Invoke(this, EventArgs.Empty);
        }
    }
}
