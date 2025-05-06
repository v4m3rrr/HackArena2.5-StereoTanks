using System.Diagnostics;

namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a light tank.
/// </summary>
public class LightTank : Tank
{
    private const int RadarRegenTicks = 200;

    /// <summary>
    /// Initializes a new instance of the <see cref="LightTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turretDirection">The direction of the turret.</param>
    /// <param name="owner">The owner of the tank.</param>
    internal LightTank(int x, int y, Direction direction, Direction turretDirection, Player owner)
        : base(x, y, owner.Id, direction)
    {
        this.Owner = owner;
        this.Turret = new LightTurret(this, turretDirection);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LightTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="ownerId">The id of the owner.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turret">The turret of the tank.</param>
    internal LightTank(int x, int y, string ownerId, Direction direction, Turret turret)
        : base(x, y, ownerId, direction)
    {
        Debug.Assert(turret is LightTurret, "The turret must be a light turret.");
        this.Turret = (LightTurret)turret;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LightTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="ownerId">The id of the owner.</param>
    /// <param name="health">The health of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="turret">The turret of the tank.</param>
    /// <param name="remainingTicksToRadar">The remaining ticks to regenerate the radar.</param>
    internal LightTank(int x, int y, string ownerId, int health, Direction direction, Turret turret, int? remainingTicksToRadar)
        : this(x, y, ownerId, direction, turret)
    {
        this.Health = health;
        this.RemainingTicksToRadar = remainingTicksToRadar;
    }

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    public new LightTurret Turret
    {
        get => (LightTurret)base.Turret;
        private set => base.Turret = value;
    }

    /// <summary>
    /// Gets the radar regeneration progress.
    /// </summary>
    /// <value>
    /// The regeneration progress of the radar as a value between 0 and 1.
    /// </value>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead.
    /// </remarks>
    public float? RadarRegenProgress => this.RemainingTicksToRadar is not null
        ? (float)(RadarRegenTicks - this.RemainingTicksToRadar) / RadarRegenTicks
        : null;

    /// <summary>
    /// Gets the remaining ticks to regenerate the radar.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead
    /// or the radar is fully charged.
    /// </remarks>
    public int? RemainingTicksToRadar { get; private set; } = RadarRegenTicks;

    /// <summary>
    /// Gets a value indicating whether the tank has a fully charged radar.
    /// </summary>
    public bool HasRadar => this.RemainingTicksToRadar is null && !this.Owner.IsDead;

    /// <summary>
    /// Gets a value indicating whether the player is using radar.
    /// </summary>
    public bool IsUsingRadar { get; internal set; }

    /// <inheritdoc/>
    public override TankType Type => TankType.Light;

#if SERVER

    /// <summary>
    /// Tries to use the radar.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the radar was used;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The radar is used only if the tank
    /// has a radar and is not stunned with the
    /// <see cref="StunBlockEffect.AbilityUse"/> block effect.
    /// </remarks>
    public bool TryUseRadar()
    {
        if (!this.HasRadar || this.IsBlockedByStun(StunBlockEffect.AbilityUse))
        {
            return false;
        }

        this.RemainingTicksToRadar = RadarRegenTicks;
        this.IsUsingRadar = true;

        return true;
    }

#endif

#if SERVER && DEBUG

    /// <inheritdoc/>
    public override void ChargeAbility(AbilityType abilityType)
    {
        if (abilityType == AbilityType.UseRadar)
        {
            this.RemainingTicksToRadar = null;
        }

        this.Turret.ChargeAbility(abilityType);
    }

#endif

    /// <inheritdoc/>
    internal override void UpdateAbilitiesCooldown()
    {
        if (this.RemainingTicksToRadar is not null)
        {
            if (--this.RemainingTicksToRadar <= 0)
            {
                this.RemainingTicksToRadar = null;
            }
        }

        this.Turret.RegenerateDoubleBulletCooldown();
    }

    /// <inheritdoc/>
    internal override void UpdateFrom(Tank tank)
    {
        if (tank is not LightTank light)
        {
            throw new ArgumentException("The tank must be a light tank.", nameof(tank));
        }

        base.UpdateFrom(tank);
        this.RemainingTicksToRadar = light.RemainingTicksToRadar;
        this.IsUsingRadar = light.IsUsingRadar;
    }
}

#endif
