namespace GameServer.Enums;

#if STEREO

/// <summary>
/// Represents the possible actions to take in a pathfinding algorithm.
/// </summary>
internal enum PathAction
{
    /// <summary>
    /// Moves the tank forward.
    /// </summary>
    MoveForward,

    /// <summary>
    /// Moves the tank backward.
    /// </summary>
    MoveBackward,

    /// <summary>
    /// Rotates the tank left.
    /// </summary>
    RotateLeft,

    /// <summary>
    /// Rotates the tank right.
    /// </summary>
    RotateRight,
}

#endif
