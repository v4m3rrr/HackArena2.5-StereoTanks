namespace GameLogic.Networking;

/// <summary>
/// Represents a packet type.
/// </summary>
public enum PacketType
{
    /// <summary>
    /// An unknown packet type.
    /// </summary>
    Unknown,

    /// <summary>
    /// The ping packet type.
    /// </summary>
    Ping,

    /// <summary>
    /// The pong packet type.
    /// </summary>
    Pong,

    /// <summary>
    /// The tank movement packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to move a tank.
    /// </remarks>
    TankMovement,

    /// <summary>
    /// The tank rotation packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to rotate a tank and/or its turret.
    /// </remarks>
    TankRotation,

    /// <summary>
    /// The tank shoot packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to shoot from a tank.
    /// </remarks>
    TankShoot,

    /// <summary>
    /// The game data packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to send game data like id, join code, etc.
    /// </remarks>
    GameData,

    /// <summary>
    /// The game state packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to send the game state.
    /// </remarks>
    GameState,

#if DEBUG
    /// <summary>
    /// The shoot all packet type.
    /// </summary>
    /// <remarks>
    /// This packet is used to shoot from all tanks.
    /// It is only for debugging purposes.
    /// </remarks>
    ShootAll,
#endif
}