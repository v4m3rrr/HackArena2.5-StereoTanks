using System.Drawing;
using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents a tank turret.
/// </summary>
public class TankTurret
{
    /// <summary>
    /// The maximum number of bullets a tank can have.
    /// </summary>
    public const int MaxBulletCount = 3;

    private const int BulletRegenTicks = 10;

    private readonly Tank tank;

    private int ticksUntilBulletRegen = BulletRegenTicks;

    /// <summary>
    /// Initializes a new instance of the <see cref="TankTurret"/> class.
    /// </summary>
    /// <param name="tank">The tank that owns the turret.</param>
    internal TankTurret(Tank tank)
    {
        this.tank = tank;
    }

    [JsonConstructor]
    private TankTurret()
    {
        this.tank ??= default!;
    }

    /// <summary>
    /// Occurs when the tank shoots a bullet.
    /// </summary>
    public event Action<Bullet>? Shot;

    /// <summary>
    /// Gets the direction of the turret.
    /// </summary>
    [JsonProperty]
    public Direction Direction { get; private set; } = EnumUtils.Random<Direction>();

    /// <summary>
    /// Gets the number of bullets the tank has.
    /// </summary>
    [JsonProperty]
    public int BulletCount { get; private set; } = MaxBulletCount;

    /// <summary>
    /// Gets the bullet regeneration progress.
    /// </summary>
    /// <remarks>
    /// The progress is a value between 0 and 1.
    /// </remarks>
    [JsonProperty]
    public float BulletRegenProgress { get; private set; }

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

        Point position = this.Direction switch
        {
            Direction.Up => new(this.tank.X, this.tank.Y - 1),
            Direction.Down => new(this.tank.X, this.tank.Y + 1),
            Direction.Left => new(this.tank.X - 1, this.tank.Y),
            Direction.Right => new(this.tank.X + 1, this.tank.Y),
            _ => throw new NotImplementedException(),
        };

        var bullet = new Bullet(this.tank.Owner)
        {
            X = position.X,
            Y = position.Y,
            Speed = 2,
            Direction = this.Direction,
        };

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
        if (this.tank.IsDead || this.BulletCount >= MaxBulletCount)
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
