namespace GameLogic;

/// <summary>
/// Represents a laser firing ability owned by a turret.
/// </summary>
/// <param name="stunSystem">The stun system used to check if the ability is blocked.</param>
internal sealed class LaserAbility(StunSystem stunSystem)
#if STEREO
    : IRegenerable, IAbility
#else
    : IAbility
#endif
{
#if STEREO
    private int? remainingRegenerationTicks = null;
#endif

    /// <summary>
    /// Gets the turret that owns this ability.
    /// </summary>
    public Turret Turret { get; init; } = default!;

    /// <inheritdoc/>
    public bool CanUse
        => !this.Turret.Tank.IsDead
#if STEREO
        && this.remainingRegenerationTicks is null
#else
        && this.Turret.Tank.SecondaryItemType is SecondaryItemType.Laser
#endif
        && !stunSystem.IsBlocked(this.Turret.Tank, StunBlockEffect.AbilityUse);

#if STEREO

    /// <inheritdoc/>
    public int TotalRegenerationTicks => 400;

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

#endif

    /// <inheritdoc/>
    public void Use()
    {
#if STEREO
        this.remainingRegenerationTicks = this.TotalRegenerationTicks;
#else
        this.Turret.Tank.SecondaryItemType = null;
#endif
    }

#if STEREO

    /// <inheritdoc/>
    public void RegenerateTick()
    {
        RegenerationUtils.UpdateRegenerationProcess(ref this.remainingRegenerationTicks);
    }

    /// <inheritdoc/>
    public void RegenerateFull()
    {
        this.remainingRegenerationTicks = null;
    }

#endif

    /// <summary>
    /// Updates the ability from another instance.
    /// </summary>
    /// <param name="snapshot">The ability to update from.</param>
    public void UpdateFrom(LaserAbility snapshot)
    {
#if STEREO
        this.remainingRegenerationTicks = snapshot.RemainingRegenerationTicks;
#endif
    }
}
