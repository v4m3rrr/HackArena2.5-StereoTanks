namespace GameLogic;

/// <summary>
/// Interface for objects that can stun a tank.
/// </summary>
internal interface IStunEffect
{
    /// <summary>
    /// Gets the number of ticks that the stun effect lasts.
    /// </summary>
    int StunTicks { get; }

    /// <summary>
    /// Gets the stun block effect.
    /// </summary>
    StunBlockEffect StunBlockEffect { get; }
}
