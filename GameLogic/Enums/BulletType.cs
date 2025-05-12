namespace GameLogic;

/// <summary>
/// Represents a bullet type.
/// </summary>
public enum BulletType
{
    /// <summary>
    /// Basic bullet type.
    /// </summary>
    Basic = 0,

    /// <summary>
    /// Double bullet type.
    /// </summary>
    Double = 1,

#if STEREO

    /// <summary>
    /// Healing bullet type.
    /// </summary>
    Healing = 2,

    /// <summary>
    /// Stun bullet type.
    /// </summary>
    Stun = 3,

#endif
}
