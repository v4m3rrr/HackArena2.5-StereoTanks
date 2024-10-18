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
    /// <param name="x">The x coordinate of the double bullet.</param>
    /// <param name="y">The y coordinate of the double bullet.</param>
    /// <param name="direction">The direction of the double bullet.</param>
    /// <param name="speed">The speed of the double bullet per second.</param>
    /// <remarks>
    /// This constructor should be used when creating a bullet
    /// from player perspective, because they shouldn't know
    /// the <see cref="Bullet.ShooterId"/>, <see cref="Bullet.Shooter"/>
    /// and <see cref="Bullet.Damage"/> (these will be set to <see langword="null"/>).
    /// </remarks>
    internal DoubleBullet(int id, int x, int y, Direction direction, float speed)
        : base(id, x, y, direction, speed)
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
    /// <remarks>
    /// <para>
    /// This constructor should be used when creating a bullet
    /// from the server or spectator perspective, because they know
    /// all the properties of the bullet.
    /// </para>
    /// <para>
    /// This constructor does not set the <see cref="Bullet.Shooter"/> property.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    internal DoubleBullet(int id, int x, int y, Direction direction, float speed, int damage, string shooterId)
        : base(id, x, y, direction, speed, damage, shooterId)
    {
    }

    /// <inheritdoc/>
    internal override BulletType Type => BulletType.Double;

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
        return this.Equals(obj as DoubleBullet);
    }

    /// <inheritdoc cref="Equals(object)"/>
    /// <remarks>
    /// The objects are considered equal if they have the same id.
    /// </remarks>
    public bool Equals(DoubleBullet? other)
    {
        return this.Id == other?.Id;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    /// <remarks>
    /// The hash code is based on the bullet's id.
    /// </remarks>
    public override int GetHashCode()
    {
        return this.Id;
    }
}
