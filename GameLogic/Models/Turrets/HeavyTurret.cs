namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a turret for a heavy tank.
/// </summary>
public class HeavyTurret : Turret
{
    private const int LaserRegenTicks = 500;
    private const int LaserDamage = 80;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTurret"/> class.
    /// </summary>
    /// <param name="tank">The tank that owns the turret.</param>
    /// <param name="direction">The direction of the turret.</param>
    internal HeavyTurret(HeavyTank tank, Direction direction)
        : base(tank, direction)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTurret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    internal HeavyTurret(Direction direction)
        : base(direction)
    {
        this.Tank = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTurret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    /// <param name="bulletCount">The number of bullets the turret has.</param>
    /// <param name="remainingTicksToBullet">The remaining ticks to regenerate the bullet.</param>
    /// <param name="remainingTicksToLaser">The remaining ticks to regenerate the laser.</param>
    internal HeavyTurret(Direction direction, int bulletCount, int? remainingTicksToBullet, int? remainingTicksToLaser)
        : base(direction, bulletCount, remainingTicksToBullet)
    {
        this.Tank = null!;
        this.RemainingTicksToLaser = remainingTicksToLaser;
    }

    /// <summary>
    /// Occurs when the tank used a laser.
    /// </summary>
    /// <remarks>
    /// The object is a list of lasers, one for each tile.
    /// </remarks>
    public event Action<List<Laser>>? LaserUsed;

    /// <summary>
    /// Gets the laser regeneration progress.
    /// </summary>
    /// <value>
    /// The regeneration progress of the laser as a value between 0 and 1.
    /// </value>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead
    /// or the laser is fully charged.
    /// </remarks>
    public float? LaserRegenProgress => this.RemainingTicksToLaser is not null
        ? 1f - (this.RemainingTicksToLaser / (float)LaserRegenTicks)
        : null;

    /// <summary>
    /// Gets the remaining ticks to regenerate the bullet.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead
    /// or the laser is fully charged.
    /// </remarks>
    public int? RemainingTicksToLaser { get; private set; } = LaserRegenTicks;

    /// <summary>
    /// Gets a value indicating whether the tank has bullets.
    /// </summary>
    public bool HasLaser => this.RemainingTicksToLaser is null && !this.Tank.IsDead;

#if DEBUG && STEREO

    /// <inheritdoc/>
    internal override void ChargeAbility(AbilityType abilityType)
    {
        base.ChargeAbility(abilityType);

        if (abilityType is not AbilityType.UseLaser)
        {
            return;
        }

        this.RemainingTicksToLaser = null;
    }

#endif

    /// <summary>
    /// Tries to use a laser.
    /// </summary>
    /// <param name="walls">The walls on the grid.</param>
    /// <returns>
    /// The laser that was shot (a list of lasers, one for each tile);
    /// <see langword="null"/> if the tank has no laser or is stunned
    /// with the <see cref="StunBlockEffect.AbilityUse"/> effect.
    /// </returns>
    internal List<Laser>? TryUseLaser(Wall?[,] walls)
    {
        if (!this.HasLaser || this.Tank.IsBlockedByStun(StunBlockEffect.AbilityUse))
        {
            return null;
        }

        var lasers = this.UseLaser(walls, LaserDamage);
        this.RemainingTicksToLaser = LaserRegenTicks;

        this.LaserUsed?.Invoke(lasers);
        this.Tank.Stun(lasers);

        return lasers;
    }

    /// <summary>
    /// Decreases the remaining ticks to regenerate the laser.
    /// </summary>
    /// <remarks>
    /// If the remaining ticks to regenerate the laser
    /// is less than or equal to 0, it is set to <see langword="null"/>.
    /// </remarks>
    internal void RegenerateLaserCooldown()
    {
        if (this.RemainingTicksToLaser is not null)
        {
            if (--this.RemainingTicksToLaser <= 0)
            {
                this.RemainingTicksToLaser = null;
            }
        }
    }

    /// <inheritdoc/>
    internal override void UpdateFrom(Turret turret)
    {
        if (turret is not HeavyTurret heavy)
        {
            throw new ArgumentException("The turret is not a heavy turret.", nameof(turret));
        }

        base.UpdateFrom(turret);
        this.RemainingTicksToLaser = heavy.RemainingTicksToLaser;
    }
}

#endif
