namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a light tank.
/// </summary>
public class LightTank : Tank
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LightTank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    /// <param name="direction">The direction of the tank.</param>
    /// <param name="owner">The owner of the tank.</param>
    internal LightTank(int x, int y, Direction direction, Player owner)
        : base(x, y, direction, owner)
    {
    }

    /// <inheritdoc/>
    public override TankType Type => TankType.Light;

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    public new LightTurret Turret
    {
        get => (LightTurret)base.Turret;
        internal set => base.Turret = value;
    }

    /// <summary>
    /// Gets or sets the radar ability of the tank.
    /// </summary>
    internal RadarAbility? Radar { get; set; } = default!;

    /// <inheritdoc/>
    internal override IEnumerable<IAbility> GetAbilities()
    {
        foreach (var ability in base.GetAbilities())
        {
            yield return ability;
        }

        if (this.Radar is not null)
        {
            yield return this.Radar;
        }
    }

#if CLIENT

    /// <inheritdoc/>
    internal override void UpdateFrom(Tank tank)
    {
        if (tank is not LightTank light)
        {
            throw new ArgumentException("The tank must be a light tank.", nameof(tank));
        }

        base.UpdateFrom(tank);

        this.Radar?.UpdateFrom(light.Radar!);
    }

#endif
}

#endif
