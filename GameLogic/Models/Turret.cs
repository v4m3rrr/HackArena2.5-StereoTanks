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
    private int ticksUntilBulletRegen = BulletRegenTicks;

    /// <summary>
    /// Initializes a new instance of the <see cref="Turret"/> class.
    /// </summary>
    /// <param name="tank">The tank that owns the turret.</param>
    internal Turret(Tank tank)
    {
        this.Tank = tank;
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
    /// <param name="bulletRegenProgress">The bullet regeneration progress.</param>
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
    internal Turret(Direction direction, int bulletCount, float? bulletRegenProgress)
    {
        this.Direction = direction;
        this.BulletCount = bulletCount;
        this.BulletRegenProgress = bulletRegenProgress;
        this.Tank = null!;
    }

    /// <summary>
    /// Occurs when the tank shoots a bullet.
    /// </summary>
    public event Action<Bullet>? Shot;

    /// <summary>
    /// Gets the direction of the turret.
    /// </summary>
    public Direction Direction { get; private set; } = EnumUtils.Random<Direction>();

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
    /// <remarks>
    /// The progress is a value between 0 and 1.
    /// </remarks>
    [JsonProperty]
    public float? BulletRegenProgress { get; private set; }

    /// <summary>
    /// Tries to shoot a bullet.
    /// </summary>
    /// <returns>
    /// The bullet that was shot,
    /// or <see langword="null"/> if the tank has no bullets.
    /// </returns>
    /// <remarks>
    /// This method creates a bullet
    /// and invokes the <see cref="Shot"/> event,
    /// if the tank has bullets.
    /// </remarks>
    public Bullet? TryShoot()
    {
        if (this.BulletCount <= 0)
        {
            return null;
        }

        var (nx, ny) = DirectionUtils.Normal(this.Direction);
        var bullet = new Bullet(
            this.Tank.X + nx,
            this.Tank.Y + ny,
            this.Direction,
            speed: 2f,
            damage: 20,
            this.Tank.Owner);

        this.BulletCount--;
        this.Shot?.Invoke(bullet);
        return bullet;
    }

    /// <summary>
    /// Rotates the turret.
    /// </summary>
    /// <param name="rotation">The rotation to apply.</param>
    public void Rotate(Rotation rotation)
    {
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
        if (this.Tank.IsDead || this.BulletCount >= MaxBulletCount)
        {
            return;
        }

        if (this.ticksUntilBulletRegen > 0)
        {
            this.BulletRegenProgress = (float)(BulletRegenTicks - --this.ticksUntilBulletRegen) / BulletRegenTicks;
        }

        if (this.ticksUntilBulletRegen <= 0)
        {
            this.BulletCount++;
            this.ticksUntilBulletRegen = BulletRegenTicks;
            this.BulletRegenProgress = 0;
        }
    }
}
