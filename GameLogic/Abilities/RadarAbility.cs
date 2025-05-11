namespace GameLogic;

/// <summary>
/// Represents a radar scan ability owned by a tank.
/// </summary>
/// <param name="stunSystem">The stun system used to check if the ability is blocked.</param>
internal sealed class RadarAbility(StunSystem stunSystem)
#if STEREO
    : IRegenerable, IAbility
#else
    : IAbility
#endif
{
    private bool isActive;

#if STEREO
    private int? remainingRegenerationTicks = null;
#endif

    /// <summary>
    /// Gets the tank that owns this ability.
    /// </summary>
    public Tank Tank { get; init; } = default!;

    /// <inheritdoc/>
    public bool CanUse
        => !this.Tank.IsDead
#if STEREO
        && this.remainingRegenerationTicks is null
#else
        && this.Tank.SecondaryItemType is SecondaryItemType.Radar
#endif
        && !stunSystem.IsBlocked(this.Tank, StunBlockEffect.AbilityUse);

    /// <summary>
    /// Gets a value indicating whether the radar ability is active.
    /// </summary>
    public bool IsActive
    {
        get => this.isActive;
        init => this.isActive = value;
    }

#if STEREO

    /// <inheritdoc/>
    public int TotalRegenerationTicks => 200;

    /// <inheritdoc/>
    public int? RemainingRegenerationTicks
    {
        get => this.remainingRegenerationTicks;
        init
        {
            if (value is not null and < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Must be greater than or equal to 0.");
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
        this.isActive = true;

#if STEREO
        this.remainingRegenerationTicks = this.TotalRegenerationTicks;
#else
        this.Tank.SecondaryItemType = null;
#endif
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.isActive = false;
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
    public void UpdateFrom(RadarAbility snapshot)
    {
        this.isActive = snapshot.IsActive;

#if STEREO
        this.remainingRegenerationTicks = snapshot.RemainingRegenerationTicks;
#endif
    }
}
