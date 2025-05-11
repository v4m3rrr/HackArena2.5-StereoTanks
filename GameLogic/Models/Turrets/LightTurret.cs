namespace GameLogic;

#if STEREO

/// <summary>
/// Represents a turret for a light tank.
/// </summary>
public class LightTurret : Turret
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LightTurret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    internal LightTurret(Direction direction)
        : base(direction)
    {
    }

    /// <summary>
    /// Gets or sets the double bullet ability of the turret.
    /// </summary>
    internal DoubleBulletAbility? DoubleBullet { get; set; } = default!;

    /// <inheritdoc/>
    internal override IEnumerable<IAbility> GetAbilities()
    {
        foreach (var ability in base.GetAbilities())
        {
            yield return ability;
        }

        if (this.DoubleBullet is not null)
        {
            yield return this.DoubleBullet;
        }
    }

#if CLIENT

    /// <inheritdoc/>
    internal override void UpdateFrom(Turret snapshot)
    {
        if (snapshot is not LightTurret light)
        {
            throw new ArgumentException("The turret is not a light turret.", nameof(snapshot));
        }

        base.UpdateFrom(snapshot);

        this.DoubleBullet?.UpdateFrom(light.DoubleBullet!);
    }

#endif
}

#endif
