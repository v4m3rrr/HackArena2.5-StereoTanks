using System.Diagnostics;

namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a heavy tank.
/// </summary>
public class HeavyTank : Tank
{
    private const int MineRegenTicks = 100;
    private const int MineDamage = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turretDirection">The direction of the turret.</param>
    /// <param name="owner">The owner of the tank.</param>
    internal HeavyTank(int x, int y, Direction direction, Direction turretDirection, Player owner)
        : base(x, y, owner.Id, direction)
    {
        this.Owner = owner;
        this.Turret = new HeavyTurret(this, turretDirection);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="ownerId">The id of the owner.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turret">The turret of the tank.</param>
    internal HeavyTank(int x, int y, string ownerId, Direction direction, Turret turret)
        : base(x, y, ownerId, direction)
    {
        Debug.Assert(turret is HeavyTurret, "The turret must be a heavy turret.");
        this.Turret = (HeavyTurret)turret;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="ownerId">The id of the owner.</param>
    /// <param name="health">The health of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turret">The turret of the tank.</param>
    /// <param name="remainingTicksToMine">The remaining ticks to regenerate the mine.</param>
    internal HeavyTank(int x, int y, string ownerId, int health, Direction direction, Turret turret, int? remainingTicksToMine)
        : this(x, y, ownerId, direction, turret)
    {
        this.Health = health;
        this.RemainingTicksToMine = remainingTicksToMine;
    }

    /// <summary>
    /// Occurs when the mine has been dropped;
    /// </summary>
    internal event EventHandler<Mine>? MineDropped;

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    public new HeavyTurret Turret
    {
        get => (HeavyTurret)base.Turret;
        private set => base.Turret = value;
    }

    /// <summary>
    /// Gets the mine regeneration progress.
    /// </summary>
    /// <value>
    /// The regeneration progress of the mine as a value between 0 and 1.
    /// </value>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead.
    /// </remarks>
    public float? MineRegenProgress => this.RemainingTicksToMine is not null
        ? (float)(MineRegenTicks - this.RemainingTicksToMine) / MineRegenTicks
        : null;

    /// <summary>
    /// Gets the remaining ticks to regenerate the mine.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead
    /// or the mine is fully charged.
    /// </remarks>
    public int? RemainingTicksToMine { get; private set; } = MineRegenTicks;

    /// <summary>
    /// Gets a value indicating whether the tank has a fully charged mine.
    /// </summary>
    public bool HasMine => this.RemainingTicksToMine is null && !this.Owner.IsDead;

    /// <inheritdoc/>
    public override TankType Type => TankType.Heavy;

#if SERVER

    /// <summary>
    /// Tries to drop a mine.
    /// </summary>
    /// <returns>
    /// The mine that was dropped, or <see langword="null"/>
    /// if the tank is stunned with the <see cref="StunBlockEffect.AbilityUse"/>
    /// block effect or the tank doesn't have a mine.
    /// </returns>
    public Mine? TryDropMine()
    {
        if (!this.HasMine)
        {
            return null;
        }

        if (this.IsBlockedByStun(StunBlockEffect.AbilityUse))
        {
            return null;
        }

        var (nx, ny) = DirectionUtils.Normal(this.Direction);
        var mine = new Mine(
            this.X - nx,
            this.Y - ny,
            MineDamage,
            this.Owner);

        this.RemainingTicksToMine = MineRegenTicks;
        this.MineDropped?.Invoke(this, mine);

        return mine;
    }

#endif

#if SERVER && DEBUG

    /// <inheritdoc/>
    public override void ChargeAbility(AbilityType abilityType)
    {
        if (abilityType == AbilityType.DropMine)
        {
            this.RemainingTicksToMine = null;
        }

        this.Turret.ChargeAbility(abilityType);
    }

#endif

    /// <inheritdoc/>
    internal override void UpdateAbilitiesCooldown()
    {
        if (this.RemainingTicksToMine is not null)
        {
            if (--this.RemainingTicksToMine <= 0)
            {
                this.RemainingTicksToMine = null;
            }
        }

        this.Turret.RegenerateLaserCooldown();
    }

    /// <inheritdoc/>
    internal override void UpdateFrom(Tank tank)
    {
        if (tank is not HeavyTank heavy)
        {
            throw new ArgumentException("The tank must be a heavy tank.", nameof(tank));
        }

        base.UpdateFrom(tank);
        this.RemainingTicksToMine = heavy.RemainingTicksToMine;
    }
}

#endif
