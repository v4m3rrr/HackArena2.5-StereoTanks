namespace GameLogic;

#if STEREO
/// <summary>
/// Represents a base class for tank turrets.
/// </summary>
public abstract class Turret
#else
/// <summary>
/// Represents a tank turret.
/// </summary>
public class Turret
#endif
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Turret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    internal Turret(Direction direction)
    {
        this.Direction = direction;
    }

    /// <summary>
    /// Gets the direction of the turret.
    /// </summary>
    public Direction Direction { get; internal set; }

    /// <summary>
    /// Gets the tank that owns the turret.
    /// </summary>
    public Tank Tank { get; internal set; } = default!;

    /// <summary>
    /// Gets or sets the bullet ability of the turret.
    /// </summary>
    internal BulletAbility? Bullet { get; set; }

#if !STEREO

    /// <summary>
    /// Gets or sets the double bullet ability of the turret.
    /// </summary>
    internal DoubleBulletAbility? DoubleBullet { get; set; }

    /// <summary>
    /// Gets or sets the laser ability of the turret.
    /// </summary>
    internal LaserAbility? Laser { get; set; }

#endif

#if STEREO

    /// <summary>
    /// Gets or sets the healing bullet ability of the turret.
    /// </summary>
    internal HealingBulletAbility? HealingBullet { get; set; }

    /// <summary>
    /// Gets or sets the stun bullet ability of the turret.
    /// </summary>
    internal StunBulletAbility? StunBullet { get; set; }

#endif

    /// <summary>
    /// Returns all active abilities owned by this turret.
    /// </summary>
    /// <returns>
    /// A collection of active abilities.
    /// </returns>
    internal virtual IEnumerable<IAbility> GetAbilities()
    {
        if (this.Bullet is not null)
        {
            yield return this.Bullet;
        }
#if STEREO

        if (this.HealingBullet is not null)
        {
            yield return this.HealingBullet;
        }

        if (this.StunBullet is not null)
        {
            yield return this.StunBullet;
        }

#else

        if (this.DoubleBullet is not null)
        {
            yield return this.DoubleBullet;
        }

        if (this.Laser is not null)
        {
            yield return this.Laser;
        }

#endif
    }

    /// <summary>
    /// Updates the turret from another turret.
    /// </summary>
    /// <param name="snapshot">The turret to update from.</param>
    internal virtual void UpdateFrom(Turret snapshot)
    {
        this.Direction = snapshot.Direction;
        this.Bullet?.UpdateFrom(snapshot.Bullet!);
#if STEREO
        this.HealingBullet?.UpdateFrom(snapshot.HealingBullet!);
        this.StunBullet?.UpdateFrom(snapshot.StunBullet!);
#else
        this.DoubleBullet?.UpdateFrom(snapshot.DoubleBullet!);
        this.Laser?.UpdateFrom(snapshot.Laser!);
#endif
    }
}
