namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a heavy tank.
/// </summary>
public class HeavyTank : Tank
{
    /// <summary>
    /// The number of ticks required for the tank to regenerate.
    /// </summary>
    public const int RegenerationTicks = 55;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="owner">The owner of the tank.</param>
    internal HeavyTank(int x, int y, Direction direction, Player owner)
        : base(x, y, direction, owner)
    {
        this.Owner = owner;
    }

    /// <inheritdoc/>
    public override TankType Type => TankType.Heavy;

    /// <inheritdoc/>
    public override int TotalRegenerationTicks => RegenerationTicks;

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    public new HeavyTurret Turret
    {
        get => (HeavyTurret)base.Turret;
        internal set => base.Turret = value;
    }

    /// <summary>
    /// Gets or sets the mine ability of the tank.
    /// </summary>
    internal MineAbility? Mine { get; set; }

    /// <inheritdoc/>
    internal override IEnumerable<IAbility> GetAbilities()
    {
        foreach (var ability in base.GetAbilities())
        {
            yield return ability;
        }

        if (this.Mine is not null)
        {
            yield return this.Mine;
        }
    }

#if CLIENT

    /// <inheritdoc/>
    internal override void UpdateFrom(Tank tank)
    {
        if (tank is not HeavyTank heavy)
        {
            throw new ArgumentException("The tank must be a heavy tank.", nameof(tank));
        }

        base.UpdateFrom(tank);

        this.Mine?.UpdateFrom(heavy.Mine!);
    }

#endif
}

#endif
