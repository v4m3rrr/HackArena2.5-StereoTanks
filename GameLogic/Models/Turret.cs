using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents a tank turret.
/// </summary>
public class Turret
{
    /// <summary>
    /// The maximum number of bullets a tank can have.
    /// </summary>
    public const int MaxBulletCount = 3;

    private const int BulletRegenTicks = 10;
    private const int BulletDamage = 20;
    private const float BulletSpeed = 2f;

    private const int LaserDamage = 80;

    /// <summary>
    /// Initializes a new instance of the <see cref="Turret"/> class.
    /// </summary>
    /// <param name="tank">The tank that owns the turret.</param>
    /// <param name="direction">The direction of the turret.</param>
    internal Turret(Tank tank, Direction direction)
    {
        this.Tank = tank;
        this.Direction = direction;
        this.BulletCount = MaxBulletCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Turret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    /// <remarks>
    /// <para>
    /// This constructor should be used when creating a turret
    /// from player perspective, because they shouldn't know
    /// the <see cref="BulletCount"/>, <see cref="BulletRegenProgress"/>
    /// (these will be set to <see langword="null"/>).
    /// </para>
    /// <para>
    /// The <see cref="Tank"/> property is set to <see langword="null"/>.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    internal Turret(Direction direction)
    {
        this.Direction = direction;
        this.Tank = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Turret"/> class.
    /// </summary>
    /// <param name="direction">The direction of the turret.</param>
    /// <param name="bulletCount">The number of bullets the tank has.</param>
    /// <param name="remainingTicksToRegenBullet">
    /// The remaining ticks to regenerate the bullet.
    /// </param>
    /// <remarks>
    /// <para>
    /// This constructor should be used when creating a turret
    /// from the server or spectator perspective, because they know
    /// all the properties of the turret.
    /// </para>
    /// <para>
    /// The <see cref="Tank"/> property is set to <see langword="null"/>.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    internal Turret(Direction direction, int bulletCount, int? remainingTicksToRegenBullet)
    {
        this.Direction = direction;
        this.BulletCount = bulletCount;
        this.RemainingTicksToRegenBullet = remainingTicksToRegenBullet;
        this.Tank = null!;
    }

    /// <summary>
    /// Occurs when the tank shot a bullet.
    /// </summary>
    public event Action<Bullet>? BulletShot;

    /// <summary>
    /// Occurs when the tank used a laser.
    /// </summary>
    /// <remarks>
    /// The object is a list of lasers, one for each tile.
    /// </remarks>
    public event Action<List<Laser>>? LaserUsed;

    /// <summary>
    /// Gets the direction of the turret.
    /// </summary>
    public Direction Direction { get; private set; }

    /// <summary>
    /// Gets the number of bullets the tank has.
    /// </summary>
    public int? BulletCount { get; private set; }

    /// <summary>
    /// Gets the tank that owns the turret.
    /// </summary>
    /// <remarks>
    /// The setter is internal because the owner is set in the
    /// <see cref="Grid.UpdateFromGameStatePayload"/> method.
    /// </remarks>
    public Tank Tank { get; internal set; }

    /// <summary>
    /// Gets the bullet regeneration progress.
    /// </summary>
    /// <value>
    /// The regeneration progress of the bullet as a value between 0 and 1.
    /// </value>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead or has full bullets.
    /// </remarks>
    public float? BulletRegenProgress => this.RemainingTicksToRegenBullet is not null
        ? 1f - (this.RemainingTicksToRegenBullet / (float)BulletRegenTicks)
        : null;

    /// <summary>
    /// Gets the remaining ticks to regenerate the bullet.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is dead or has full bullets.
    /// </remarks>
    public int? RemainingTicksToRegenBullet { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the tank has full bullets.
    /// </summary>
    public bool HasFullBullets => this.BulletCount >= MaxBulletCount;

    /// <summary>
    /// Gets a value indicating whether the tank has bullets.
    /// </summary>
    public bool HasBullets => this.BulletCount > 0;

    /// <summary>
    /// Tries to fire a bullet.
    /// </summary>
    /// <returns>
    /// The bullet that was shot;
    /// <see langword="null"/> if the tank has no bullets or is stunned
    /// with the <see cref="StunBlockEffect.AbilityUse"/> effect.
    /// </returns>
    /// <remarks>
    /// This method creates a bullet
    /// and invokes the <see cref="BulletShot"/> event,
    /// if the tank has bullets.
    /// </remarks>
    public Bullet? TryFireBullet()
    {
        if (!this.HasBullets)
        {
            return null;
        }

        if (this.Tank.IsBlockedByStun(StunBlockEffect.AbilityUse))
        {
            return null;
        }

        var (nx, ny) = DirectionUtils.Normal(this.Direction);
        var bullet = new Bullet(
            this.Tank.X + nx,
            this.Tank.Y + ny,
            this.Direction,
            speed: BulletSpeed,
            damage: BulletDamage,
            this.Tank.Owner);

        this.BulletCount--;
        this.RemainingTicksToRegenBullet ??= BulletRegenTicks;
        this.BulletShot?.Invoke(bullet);

        return bullet;
    }

    /// <summary>
    /// Tries to fire a double bullet.
    /// </summary>
    /// <returns>
    /// The double bullet that was shot;
    /// <see langword="null"/> if the tank
    /// has no double bullet or is stunned
    /// with the <see cref="StunBlockEffect.AbilityUse"/> effect.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method creates a double bullet
    /// and invokes the <see cref="BulletShot"/> event,
    /// if the tank has double bullet.
    /// </para>
    /// </remarks>
    public DoubleBullet? TryFireDoubleBullet()
    {
        if (this.Tank.IsBlockedByStun(StunBlockEffect.AbilityUse))
        {
            return null;
        }

        if (this.Tank.SecondaryItemType is not SecondaryItemType.DoubleBullet)
        {
            return null;
        }

        var (nx, ny) = DirectionUtils.Normal(this.Direction);
        var doubleBullet = new DoubleBullet(
            this.Tank.X + nx,
            this.Tank.Y + ny,
            this.Direction,
            BulletSpeed,
            BulletDamage * 2,
            this.Tank.Owner);

        this.Tank.SecondaryItemType = null;
        this.BulletShot?.Invoke(doubleBullet);

        return doubleBullet;
    }


    /// <summary>
    /// Tries to use a laser.
    /// </summary>
    /// <param name="walls">The walls on the grid.</param>
    /// <returns>
    /// The laser that was shot (a list of lasers, one for each tile);
    /// <see langword="null"/> if the tank has no laser or is stunned
    /// with the <see cref="StunBlockEffect.AbilityUse"/> effect.
    /// </returns>
    public List<Laser>? TryUseLaser(Wall?[,] walls)
    {
        if (this.Tank.IsBlockedByStun(StunBlockEffect.AbilityUse))
        {
            return null;
        }

        if (this.Tank.SecondaryItemType is not SecondaryItemType.Laser)
        {
            return null;
        }

        var (nx, ny) = DirectionUtils.Normal(this.Direction);

        var tiles = new List<(int X, int Y)>();

        var currX = this.Tank.X + nx;
        var currY = this.Tank.Y + ny;

        while (currX >= 0 && currX < walls.GetLength(0) && currY >= 0 && currY < walls.GetLength(1))
        {
            if (walls[currX, currY] is not null)
            {
                break;
            }

            tiles.Add((currX, currY));

            currX += nx;
            currY += ny;
        }

        var lasers = new List<Laser>();
        var orientation = DirectionUtils.ToOrientation(this.Direction);

        foreach (var (x, y) in tiles)
        {
            var laser = new Laser(x, y, orientation, LaserDamage, this.Tank.Owner);
            lasers.Add(laser);
        }

        this.Tank.SecondaryItemType = null;
        this.LaserUsed?.Invoke(lasers);
        this.Tank.Stun(lasers);

        return lasers;
    }

    /// <summary>
    /// Rotates the turret.
    /// </summary>
    /// <param name="rotation">The rotation to apply.</param>
    /// <remarks>
    /// The rotation is ignored if the tank is stunned by the
    /// <see cref="StunBlockEffect.TurretRotation"/> effect.
    /// </remarks>
    public void Rotate(Rotation rotation)
    {
        if (this.Tank.IsBlockedByStun(StunBlockEffect.TurretRotation))
        {
            return;
        }

        this.Direction = rotation switch
        {
            Rotation.Left => EnumUtils.Previous(this.Direction),
            Rotation.Right => EnumUtils.Next(this.Direction),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Regenerates bullets over time.
    /// </summary>
    public void RegenerateBullets()
    {
        if (this.Tank.IsDead || this.HasFullBullets)
        {
            return;
        }

        if (--this.RemainingTicksToRegenBullet <= 0)
        {
            this.BulletCount++;
            this.RemainingTicksToRegenBullet = this.HasFullBullets ? null : BulletRegenTicks;
        }
    }
}
