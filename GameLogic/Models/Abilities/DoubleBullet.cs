namespace GameLogic;

/// <summary>
/// Represents a double bullet.
/// </summary>
public class DoubleBullet : Bullet, IEquatable<DoubleBullet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleBullet"/> class.
    /// </summary>
    /// <param name="x">The x coordinate of the double bullet.</param>
    /// <param name="y">The y coordinate of the double bullet.</param>
    /// <param name="direction">The direction of the double bullet.</param>
    /// <param name="speed">The speed of the double bullet per second.</param>
    /// <param name="damage">The damage dealt by the double bullet.</param>
    /// <param name="shooter">The tank that shot the double bullet.</param>
    /// <remarks>
    /// <para>This constructor should be used when a tank shoots a double bullet.</para>
    /// <para>The <see cref="Bullet.Id"/> property is set automatically.</para>
    /// </remarks>
    internal DoubleBullet(int x, int y, Direction direction, float speed, int damage, Player shooter)
        : base(x, y, direction, speed, damage, shooter)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleBullet"/> class.
    /// </summary>
    /// <param name="id">The id of the double bullet.</param>
    /// <param name="direction">The direction of the double bullet.</param>
    /// <param name="speed">The speed of the double bullet per second.</param>
    /// <param name="x">The x coordinate of the double bullet.</param>
    /// <param name="y">The y coordinate of the double bullet.</param>
    /// <param name="damage">The damage dealt by the double bullet.</param>
    /// <param name="shooterId">The id of the tank that shot the double bullet.</param>
    internal DoubleBullet(int id, int x, int y, Direction direction, float speed, int? damage = null, string? shooterId = null)
        : base(id, x, y, direction, speed, damage, shooterId)
    {
    }

    /// <inheritdoc/>
    internal override BulletType Type => BulletType.Double;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as DoubleBullet);
    }

    /// <inheritdoc/>
    public bool Equals(DoubleBullet? other)
    {
        return this.Id == other?.Id;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.Id;
    }
}
