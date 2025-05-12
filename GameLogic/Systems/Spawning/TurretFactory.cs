namespace GameLogic;

/// <summary>
/// Responsible for creating turret instances with bound abilities.
/// </summary>
/// <param name="stunSystem">The stun system for managing stun effects.</param>
internal sealed class TurretFactory(StunSystem stunSystem)
{
#if !STEREO

    /// <summary>
    /// Creates a turret with abilities.
    /// </summary>
    /// <param name="direction">The initial turret direction.</param>
    /// <returns>Fully initialized <see cref="Turret"/> with bound abilities.</returns>
    public Turret Create(Direction direction)
    {
        var turret = new Turret(direction);

        turret.Bullet = new BulletAbility(stunSystem)
        {
            Turret = turret,
        };

        turret.DoubleBullet = new DoubleBulletAbility(stunSystem)
        {
            Turret = turret,
        };

        turret.Laser = new LaserAbility(stunSystem)
        {
            Turret = turret,
        };

        return turret;
    }

#else

    /// <summary>
    /// Creates a light turret with double bullet and bullet regeneration ability.
    /// </summary>
    /// <param name="direction">The initial turret direction.</param>
    /// <returns>Fully initialized <see cref="LightTurret"/>.</returns>
    public LightTurret CreateLightTurret(Direction direction)
    {
        var turret = new LightTurret(direction);

        this.InitializeSharedAbilities(turret);

        turret.DoubleBullet = new DoubleBulletAbility(stunSystem)
        {
            Turret = turret,
        };

        return turret;
    }

    /// <summary>
    /// Creates a heavy turret with bullet and laser abilities.
    /// </summary>
    /// <param name="direction">The initial turret direction.</param>
    /// <returns>Fully initialized <see cref="HeavyTurret"/>.</returns>
    public HeavyTurret CreateHeavyTurret(Direction direction)
    {
        var turret = new HeavyTurret(direction);

        this.InitializeSharedAbilities(turret);

        turret.Laser = new LaserAbility(stunSystem)
        {
            Turret = turret,
        };

        return turret;
    }

    private void InitializeSharedAbilities(Turret turret)
    {
        turret.Bullet = new BulletAbility(stunSystem)
        {
            Turret = turret,
        };

        turret.HealingBullet = new HealingBulletAbility(stunSystem)
        {
            Turret = turret,
        };

        turret.StunBullet = new StunBulletAbility(stunSystem)
        {
            Turret = turret,
        };
    }

#endif
}
