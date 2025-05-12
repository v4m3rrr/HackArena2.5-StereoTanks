namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a stun bullet shooting ability owned by a turret.
/// </summary>
/// <param name="stunSystem">The stun system used to check if the ability is blocked.</param>
internal sealed class StunBulletAbility(StunSystem stunSystem)
    : IRegenerable, IBulletFiringAbility
{
    private int? remainingRegenerationTicks = null;

    /// <summary>
    /// Gets the turret that owns this ability.
    /// </summary>
    public Turret Turret { get; init; } = default!;

    /// <inheritdoc/>
    public bool CanUse
        => !this.Turret.Tank.IsDead
        && this.remainingRegenerationTicks is null
        && !stunSystem.IsBlocked(this.Turret.Tank, StunBlockEffect.AbilityUse);

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
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be greater than or equal to 0.");
            }

            this.remainingRegenerationTicks = value;
        }
    }

    /// <inheritdoc/>
    public float? RegenerationProgress => RegenerationUtils.GetRegenerationProgres(this);

    /// <inheritdoc/>
    public void Use()
    {
        this.remainingRegenerationTicks = this.TotalRegenerationTicks;
    }

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

    /// <summary>
    /// Updates the ability from another instance.
    /// </summary>
    /// <param name="snapshot">The ability to update from.</param>
    public void UpdateFrom(StunBulletAbility snapshot)
    {
        this.remainingRegenerationTicks = snapshot.RemainingRegenerationTicks;
    }
}

#endif
