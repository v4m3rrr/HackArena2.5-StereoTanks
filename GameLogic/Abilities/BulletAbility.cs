namespace GameLogic;

/// <summary>
/// Represents a basic bullet shooting ability owned by a turret.
/// </summary>
/// <param name="stunSystem">The stun system used to check if the ability is blocked.</param>
internal sealed class BulletAbility(StunSystem stunSystem) : IRegenerable, IAbility
{
    /// <summary>
    /// The maximum number of bullets that can be held at once.
    /// </summary>
    public const int MaxBullets = 3;

    private int count = MaxBullets;
    private int? remainingRegenerationTicks = null;

    /// <inheritdoc/>
    public int TotalRegenerationTicks => 10;

    /// <summary>
    /// Gets the count of bullets available for this ability.
    /// </summary>
    public int Count
    {
        get => this.count;
        init
        {
            if (value is < 0 or > MaxBullets)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be between 0 and MaxBullets.");
            }

            this.count = value;
        }
    }

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

    /// <summary>
    /// Gets the turret that owns this ability.
    /// </summary>
    public Turret Turret { get; init; } = default!;

    /// <inheritdoc/>
    public bool CanUse
        => !this.Turret.Tank.IsDead
        && this.Count > 0
        && !stunSystem.IsBlocked(this.Turret.Tank, StunBlockEffect.AbilityUse);

    /// <inheritdoc/>
    public void Use()
    {
        this.count--;
        this.remainingRegenerationTicks = this.TotalRegenerationTicks;
    }

    /// <inheritdoc/>
    public void RegenerateTick()
    {
        if (this.Count == MaxBullets)
        {
            return;
        }

        RegenerationUtils.UpdateRegenerationProcess(ref this.remainingRegenerationTicks);

        if (this.remainingRegenerationTicks is null)
        {
            this.count++;
            this.remainingRegenerationTicks = this.count < MaxBullets
                ? this.TotalRegenerationTicks
                : null;
        }
    }

    /// <inheritdoc/>
    public void RegenerateFull()
    {
        if (this.count < MaxBullets)
        {
            this.count++;

            if (this.count >= MaxBullets)
            {
                this.remainingRegenerationTicks = null;
            }
        }
    }

    /// <summary>
    /// Updates the ability from another instance.
    /// </summary>
    /// <param name="snapshot">The ability to update from.</param>
    public void UpdateFrom(BulletAbility snapshot)
    {
        this.count = snapshot.Count;
        this.remainingRegenerationTicks = snapshot.RemainingRegenerationTicks;
    }
}
