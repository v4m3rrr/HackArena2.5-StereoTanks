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

    /// <summary>
    /// The connection accepted packet type.
    /// </summary>
    ConnectionAccepted = CommunicationGroup | 0x3,

    /// <summary>
    /// The connection rejected packet type.
    /// </summary>
    ConnectionRejected = CommunicationGroup | HasPayload | 0x4,

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
    PlayerResponseActionGroup = 0x40,

    /// <summary>
    /// The tank movement packet type.
    /// </summary>
    TankMovement = PlayerResponseActionGroup | HasPayload | 0x1,

    /// <summary>
    /// The tank rotation packet type.
    /// </summary>
    TankRotation = PlayerResponseActionGroup | HasPayload | 0x2,

    /// <summary>
    /// The tank shoot packet type.
    /// </summary>
    TankShoot = PlayerResponseActionGroup | HasPayload | 0x3,

    /// <summary>
    /// The reponse pass packet type.
    /// </summary>
    ResponsePass = PlayerResponseActionGroup | HasPayload | 0x7,

#if DEBUG

    // GameState debug group (range: 0xC0 - 0xCF)

    /// <summary>
    /// The game state debug group packet type.
    /// </summary>
    GameStateDebugGroup = 0xC0,

    /// <summary>
    /// The set player score packet type.
    /// </summary>
    SetPlayerScore = GameStateDebugGroup | HasPayload | 0x1,

    // Debug group (range: 0xD0 - 0xDF)

    /// <summary>
    /// The debug group packet type.
    /// </summary>
    DebugGroup = 0xD0,

    /// <summary>
    /// The shoot all packet type (debug).
    /// </summary>
    ShootAll = DebugGroup | 0x3,

    /// <summary>
    /// The force end game packet type (debug).
    /// </summary>
    ForceEndGame = DebugGroup | 0x4,

#endif

    // Warning group (range: 0xE0 - 0xEF)

    /// <summary>
    /// The warning group packet type.
    /// </summary>
    WarningGroup = 0xE0,

    /// <summary>
    /// The warning packet type with custom message.
    /// </summary>
    CustomWarning = WarningGroup | HasPayload | 0x1,

    /// <summary>
    /// The already made movement packet type.
    /// </summary>
    PlayerAlreadyMadeActionWarning = WarningGroup | 0x2,

    /// <summary>
    /// The action ignored packet type.
    /// </summary>
    ActionIgnoredDueToDeadWarning = WarningGroup | 0x3,

    /// <summary>
    /// The slow response packet type.
    /// </summary>
    SlowResponseWarning = WarningGroup | 0x4,

    // Error group (range: 0xF0 - 0xFF)

    /// <summary>
    /// The error group packet type.
    /// </summary>
    ErrorGroup = 0xF0,

    /// <summary>
    /// The invalid packet type.
    /// </summary>
    /// <remarks>
    /// Can be merged with <see cref="HasPayload"/>.
    /// </remarks>
    InvalidPacketTypeError = ErrorGroup | 0x1,

    /// <summary>
    /// The invalid packet usage.
    /// </summary>
    /// <remarks>
    /// Can be merged with <see cref="HasPayload"/>.
    /// </remarks>
    InvalidPacketUsageError = ErrorGroup | 0x2,
}
