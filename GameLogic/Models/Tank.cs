using Newtonsoft.Json;

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
    {
        this.X = x;
        this.Y = y;

        this.Owner = owner;
        this.OwnerId = owner.Id;
        owner.Tank = this;

        this.Turret = new TankTurret(this);
    }

    [JsonConstructor]
    private Tank()
    {
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
    [JsonProperty]
    public int X { get; private set; }

    /// <summary>
    /// Gets the y coordinate of the tank.
    /// </summary>
    [JsonProperty]
    public int Y { get; private set; }

    /// <summary>
    /// Gets the health of the tank.
    /// </summary>
    [JsonProperty]
    public int Health { get; private set; } = 100;

    /// <summary>
    /// Gets the regeneration progress of the tank.
    /// </summary>
    [JsonProperty]
    public float RegenProgress { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the tank is dead.
    /// </summary>
    [JsonIgnore]
    public bool IsDead => this.Health <= 0;

    /// <summary>
    /// Gets the owner of the tank.
    /// </summary>
    /// <remarks>
    /// This property has <see cref="JsonIgnoreAttribute"/> because it
    /// is set in the <see cref="Networking.GameStatePayload.GridState"/>
    /// init property when deserializing the game state,
    /// based on the <see cref="OwnerId"/> property.
    /// </remarks>
    [JsonIgnore]
    public Player Owner { get; internal set; } = default!;

    /// <summary>
    /// Gets the direction of the tank.
    /// </summary>
    [JsonProperty]
    public Direction Direction { get; private set; } = EnumUtils.Random<Direction>();

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    [JsonProperty]
    public TankTurret Turret { get; private set; } = default!;

    /// <summary>
    /// Gets the owner ID of the tank.
    /// </summary>
    [JsonProperty]
    internal string OwnerId { get; private init; } = default!;

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
    /// Reduces the health of the tank.
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    internal void TakeDamage(int damage)
    {
        this.Health -= damage;

        if (this.Health <= 0)
        {
            this.SetPosition(-1, -1);
            this.Died?.Invoke(this, EventArgs.Empty);
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
            this.RegenProgress = 0;
            this.Regenerated?.Invoke(this, EventArgs.Empty);
        }
    }
}
