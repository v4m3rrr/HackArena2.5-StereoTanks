namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a turret for a heavy tank.
/// </summary>
public class HeavyTurret : Turret
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeavyTurret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    internal HeavyTurret(Direction direction)
        : base(direction)
    {
    }

    /// <summary>
    /// Gets or sets the laser ability of the turret.
    /// </summary>
    internal LaserAbility? Laser { get; set; }

    /// <inheritdoc/>
    internal override IEnumerable<IAbility> GetAbilities()
    {
        foreach (var ability in base.GetAbilities())
        {
            yield return ability;
        }

        if (this.Laser is not null)
        {
            yield return this.Laser;
        }
    }

#if CLIENT

    /// <inheritdoc/>
    internal override void UpdateFrom(Turret turret)
    {
        if (turret is not HeavyTurret heavy)
        {
            throw new ArgumentException("The turret is not a heavy turret.", nameof(turret));
        }

        base.UpdateFrom(turret);

        this.Laser?.UpdateFrom(heavy.Laser!);
    }

#endif
}

#endif
