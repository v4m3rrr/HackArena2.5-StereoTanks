namespace GameLogic;

/// <summary>
/// Represents the type of ability that the tank can use.
/// </summary>
public enum AbilityType
{
    /// <summary>
    /// Fires a bullet.
    /// </summary>
    FireBullet,

    /// <summary>
    /// Uses a laser.
    /// </summary>
    UseLaser,

    /// <summary>
    /// Fires double bullets.
    /// </summary>
    FireDoubleBullet,

    /// <summary>
    /// Uses a radar.
    /// </summary>
    UseRadar,

    /// <summary>
    /// Drops a mine.
    /// </summary>
    DropMine,

#if STEREO

    /// <summary>
    /// Fires a healing bullet.
    /// </summary>
    FireHealingBullet,

    /// <summary>
    /// Fires a stun bullet.
    /// </summary>
    FireStunBullet,

#endif
}
