namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a turret for a light tank.
/// </summary>
public class LightTurret : Turret
{
    private const int DoubleBulletRegenTicks = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="LightTurret"/> class.
    /// </summary>
    /// <param name="tank">The tank that owns the turret.</param>
    /// <param name="direction">The direction of the turret.</param>
    internal LightTurret(LightTank tank, Direction direction)
        : base(tank, direction)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LightTurret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    internal LightTurret(Direction direction)
        : base(direction)
    {
        this.Tank = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LightTurret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    /// <param name="bulletCount">The number of bullets the turret has.</param>
    /// <param name="remainingTicksToBullet">The remaining ticks to regenerate the bullet.</param>
    /// <param name="remainingTicksToDoubleBullet">The remaining ticks to regenerate the double bullet.</param>
    internal LightTurret(Direction direction, int bulletCount, int? remainingTicksToBullet, int? remainingTicksToDoubleBullet)
        : base(direction, bulletCount, remainingTicksToBullet)
    {
        this.Tank = null!;
        this.RemainingTicksToDoubleBullet = remainingTicksToDoubleBullet;
    }

    /// <summary>
    /// Gets the double bullet regeneration progress.
    /// </summary>
    /// <value>
    /// The regeneration progress of the double bullet as a value between 0 and 1.
    /// </value>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead
    /// or the double bullet is fully charged.
    /// </remarks>
    public float? DoubleBulletRegenProgress => this.RemainingTicksToDoubleBullet is not null
        ? 1f - (this.RemainingTicksToDoubleBullet / (float)DoubleBulletRegenTicks)
        : null;

    /// <summary>
    /// Gets the remaining ticks to regenerate the bullet.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead
    /// or the double bullet is fully charged.
    /// </remarks>
    public int? RemainingTicksToDoubleBullet { get; private set; } = DoubleBulletRegenTicks;

    /// <summary>
    /// Gets a value indicating whether the tank has bullets.
    /// </summary>
    public bool HasDoubleBullet => this.RemainingTicksToDoubleBullet is null
        && this.Tank is not null && !this.Tank.IsDead;

    /// <summary>
    /// Tries to fire a double bullet.
    /// </summary>
    /// <returns>
    /// The double bullet that was shot;
    /// <see langword="null"/> if the tank
    /// has no double bullet or is stunned
    /// with the <see cref="StunBlockEffect.AbilityUse"/> effect.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method creates a double bullet
    /// and invokes the <see cref="Turret.BulletShot"/> event,
    /// if the tank shot a double bullet.
    /// </para>
    /// </remarks>
    public DoubleBullet? TryFireDoubleBullet()
    {
        if (!this.HasDoubleBullet)
        {
            return null;
        }

        if (this.Tank.IsBlockedByStun(StunBlockEffect.AbilityUse))
        {
            return null;
        }

        var doubleBullet = this.FireDoubleBullet();
        this.RemainingTicksToDoubleBullet = DoubleBulletRegenTicks;
        this.OnBulletShot(doubleBullet);

        return doubleBullet;
    }

#if DEBUG && STEREO

    /// <inheritdoc/>
    internal override void ChargeAbility(AbilityType abilityType)
    {
        base.ChargeAbility(abilityType);

        if (abilityType is not AbilityType.FireDoubleBullet)
        {
            return;
        }

        this.RemainingTicksToDoubleBullet = null;
    }

#endif

    /// <summary>
    /// Decreases the remaining ticks to regenerate the double bullet.
    /// </summary>
    /// <remarks>
    /// If the remaining ticks to regenerate the double bullet
    /// is less than or equal to 0, it is set to <see langword="null"/>.
    /// </remarks>
    internal void RegenerateDoubleBulletCooldown()
    {
        if (this.RemainingTicksToDoubleBullet is not null)
        {
            if (--this.RemainingTicksToDoubleBullet <= 0)
            {
                this.RemainingTicksToDoubleBullet = null;
            }
        }
    }

    /// <inheritdoc/>
    internal override void UpdateFrom(Turret turret)
    {
        if (turret is not LightTurret light)
        {
            throw new ArgumentException("The turret is not a light turret.", nameof(turret));
        }

        base.UpdateFrom(turret);
        this.RemainingTicksToDoubleBullet = light.RemainingTicksToDoubleBullet;
    }
}

#endif
