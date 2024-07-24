using System.Drawing;
using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents a tank.
/// </summary>
public class Tank : IEquatable<Tank>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    internal Tank(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    [JsonConstructor]
    private Tank()
    {
    }

    /// <summary>
    /// Occurs when the tank shoots a bullet.
    /// </summary>
    public event Action<Bullet>? OnShoot;

    /// <summary>
    /// Gets the x coordinate of the tank.
    /// </summary>
    [JsonProperty]
    public int X { get; private set; }

    /// <summary>
    /// Gets the y coordinate of the tank.
    /// </summary>
    [JsonProperty]
    public int Y { get; private set; }

    /// <summary>
    /// Gets the color of the tank.
    /// </summary>
    [JsonProperty]
    public uint Color { get; internal set; }

    /// <summary>
    /// Gets the health of the tank.
    /// </summary>
    [JsonProperty]
    public int Health { get; private set; } = 100;

    /// <summary>
    /// Gets the direction of the tank.
    /// </summary>
    [JsonProperty]
    public Direction Direction { get; private set; } = EnumUtils.Random<Direction>();

    /// <summary>
    /// Gets the turret of the tank.
    /// </summary>
    [JsonProperty]
    public TankTurret Turret { get; private set; } = new();

    /// <summary>
    /// Rotates the tank.
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
    /// Shoots a bullet.
    /// </summary>
    /// <returns>The bullet that was shot.</returns>
    /// <remarks>
    /// This method creates a bullet
    /// and invokes the <see cref="OnShoot"/> event.
    /// </remarks>
    public Bullet Shoot()
    {
        var direction = this.Turret.Direction;
        Point position = direction switch
        {
            Direction.Up => new(this.X, this.Y - 1),
            Direction.Down => new(this.X, this.Y + 1),
            Direction.Left => new(this.X - 1, this.Y),
            Direction.Right => new(this.X + 1, this.Y),
            _ => throw new NotImplementedException(),
        };

        var bullet = new Bullet
        {
            X = position.X,
            Y = position.Y,
            Speed = 2,
            Direction = this.Turret.Direction,
            Shooter = this,
        };

        this.OnShoot?.Invoke(bullet);
        return bullet;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is equal to the current object;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Tank);
    }

    /// <inheritdoc cref="Equals(object)"/>
    public bool Equals(Tank? other)
    {
        return this.Color == other?.Color;
    }

    /// <summary>
    /// Gets the hash code of the tank.
    /// </summary>
    /// <returns>The hash code of the tank.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Color);
    }

    /// <summary>
    /// Reduces the health of the tank.
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    internal void TakeDamage(int damage)
    {
        this.Health -= damage;
    }

    /// <summary>
    /// Sets the position of the tank.
    /// </summary>
    /// <param name="x">The x coordinate of the tank.</param>
    /// <param name="y">The y coordinate of the tank.</param>
    internal void SetPosition(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}
