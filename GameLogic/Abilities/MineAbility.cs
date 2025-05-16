namespace GameLogic;

/// <summary>
/// Represents a mine-dropping ability owned by a tank.
/// </summary>
/// <param name="stunSystem">The stun system used to check if the ability is blocked.</param>
internal sealed class MineAbility(StunSystem stunSystem)
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
    /// Gets the tank that owns this ability.
    /// </summary>
    public Tank Tank { get; init; } = default!;

    /// <inheritdoc/>
    public bool CanUse
        => !this.Tank.IsDead
#if STEREO
        && this.remainingRegenerationTicks is null
#else
        && this.Tank.SecondaryItemType is SecondaryItemType.Mine
#endif
        && !stunSystem.IsBlocked(this.Tank, StunBlockEffect.AbilityUse);

#if STEREO

    /// <inheritdoc/>
    public int TotalRegenerationTicks => 80;

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
        this.Tank.SecondaryItemType = null;
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
    public void UpdateFrom(MineAbility snapshot)
    {
#if STEREO
        this.remainingRegenerationTicks = snapshot.RemainingRegenerationTicks;
#endif
    }
}
