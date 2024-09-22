namespace GameLogic.Networking;

/// <summary>
/// Represents a packet type.
/// </summary>
public enum PacketType
{
    /// <summary>
    /// An unknown packet type.
    /// </summary>
    Unknown = 0x0,

    /// <summary>
    /// The mask for packet type
    /// indicating that it has a payload.
    /// </summary>
    HasPayload = 0x8,

    // Communication group (range: 0x10 - 0x1F)

    /// <summary>
    /// The communication group packet type.
    /// </summary>
    CommunicationGroup = 0x10,

    /// <summary>
    /// The ping packet type.
    /// </summary>
    Ping = CommunicationGroup | 0x1,

    /// <summary>
    /// The pong packet type.
    /// </summary>
    Pong = CommunicationGroup | 0x2,

    // Lobby group (range: 0x20 - 0x2F)

    /// <summary>
    /// The lobby group packet type.
    /// </summary>
    LobbyGroup = 0x20,

    /// <summary>
    /// The lobby data packet type.
    /// </summary>
    LobbyData = LobbyGroup | HasPayload | 0x1,

    /// <summary>
    /// The lobby deleted packet type.
    /// </summary>
    LobbyDeleted = LobbyGroup | 0x2,

    // GameState group (range: 0x30 - 0x3F)

    /// <summary>
    /// The game state packet type.
    /// </summary>
    GameStateGroup = 0x30,

    /// <summary>
    /// The game start packet type.
    /// </summary>
    GameStart = GameStateGroup | 0x1,

    /// <summary>
    /// The game state packet type.
    /// </summary>
    GameState = GameStateGroup | HasPayload | 0x2,

    /// <summary>
    /// The game end packet type.
    /// </summary>
    GameEnd = GameStateGroup | HasPayload | 0x3,

    // Player response group (range: 0x40 - 0x4F)

    /// <summary>
    /// The player response group packet type.
    /// </summary>
    PlayerResponseGroup = 0x40,

    /// <summary>
    /// The tank movement packet type.
    /// </summary>
    TankMovement = PlayerResponseGroup | HasPayload | 0x1,

    /// <summary>
    /// The tank rotation packet type.
    /// </summary>
    TankRotation = PlayerResponseGroup | HasPayload | 0x2,

    /// <summary>
    /// The tank shoot packet type.
    /// </summary>
    TankShoot = PlayerResponseGroup | HasPayload | 0x3,

#if DEBUG

    // Debug group (range: 0xD0 - 0xDF)

    /// <summary>
    /// The debug group packet type.
    /// </summary>
    DebugGroup = 0xD0,

    /// <summary>
    /// The shoot all packet type (debug).
    /// </summary>
    ShootAll = DebugGroup | 0x3,

#endif

    // Warning group (range: 0xE0 - 0xEF)

    /// <summary>
    /// The warning group packet type.
    /// </summary>
    WarningGroup = 0xE0,

    /// <summary>
    /// The invalid packet type.
    /// </summary>
    Invalid = WarningGroup | 0x1,

    /// <summary>
    /// The already made movement packet type.
    /// </summary>
    AlreadyMadeMovement = WarningGroup | 0x2,
}
