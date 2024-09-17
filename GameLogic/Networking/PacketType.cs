namespace GameLogic.Networking;

/// <summary>
/// Represents a packet type.
/// </summary>
public enum PacketType
{
    /// <summary>
    /// An unknown packet type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The ping packet type.
    /// </summary>
    Ping = 1,

    /// <summary>
    /// The pong packet type.
    /// </summary>
    Pong = 2,

    /// <summary>
    /// The tank movement packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to move a tank.
    /// </remarks>
    TankMovement = 11,

    /// <summary>
    /// The tank rotation packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to rotate a tank and/or its turret.
    /// </remarks>
    TankRotation = 12,

    /// <summary>
    /// The tank shoot packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to shoot from a tank.
    /// </remarks>
    TankShoot = 13,

    /// <summary>
    /// The game state packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to send the game state.
    /// </remarks>
    GameState = 21,

    /// <summary>
    /// The lobby data packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to send the lobby data.
    /// </remarks>
    LobbyData = 31,

    /// <summary>
    /// The lobby deleted packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to notify that the lobby was deleted.
    /// </remarks>
    LobbyDeleted = 32,

    /// <summary>
    /// The game start packet type.
    /// </summary>
    GameStart = 33,

#if DEBUG
    /* Debug packets should havea value between 91 and 99. */

    /// <summary>
    /// The shoot all packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to shoot from all tanks.
    /// It is only for debugging purposes.
    /// </remarks>
    ShootAll = 91,
#endif
}
