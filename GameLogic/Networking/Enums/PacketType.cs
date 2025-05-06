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
    LobbyDataRequest = LobbyGroup | 0x2,

    // GameState group (range: 0x30 - 0x3F)

    /// <summary>
    /// The game state packet type.
    /// </summary>
    GameStateGroup = 0x30,

    /// <summary>
    /// The game state packet type.
    /// </summary>
    GameState = GameStateGroup | HasPayload | 0x2,

    /// <summary>
    /// The ready to receive game state packet type.
    /// </summary>
    ReadyToReceiveGameState = GameStateGroup | 0x5,

    // Player response group (range: 0x40 - 0x4F)

    /// <summary>
    /// The player response group packet type.
    /// </summary>
    PlayerResponseActionGroup = 0x40,

    /// <summary>
    /// The move packet type.
    /// </summary>
    Movement = PlayerResponseActionGroup | HasPayload | 0x1,

    /// <summary>
    /// The rotation packet type.
    /// </summary>
    Rotation = PlayerResponseActionGroup | HasPayload | 0x2,

    /// <summary>
    /// The use packet type.
    /// </summary>
    AbilityUse = PlayerResponseActionGroup | HasPayload | 0x3,

#if STEREO

    /// <summary>
    /// The go to packet type.
    /// </summary>
    GoTo = PlayerResponseActionGroup | HasPayload | 0x6,

#endif

    /// <summary>
    /// The pass packet type.
    /// </summary>
    Pass = PlayerResponseActionGroup | HasPayload | 0x7,

    // Game status group (range: 0x50 - 0x5F)

    /// <summary>
    /// The game status group packet type.
    /// </summary>
    GameStatusGroup = 0x50,

    /// <summary>
    /// The game not started packet type.
    /// </summary>
    GameNotStarted = GameStatusGroup | 0x1,

    /// <summary>
    /// The game started packet type.
    /// </summary>
    GameStarting = GameStatusGroup | 0x2,

    /// <summary>
    /// The game started packet type.
    /// </summary>
    GameStarted = GameStatusGroup | 0x3,

    /// <summary>
    /// The game started packet type.
    /// </summary>
    GameInProgress = GameStatusGroup | 0x4,

    /// <summary>
    /// The game ended packet type.
    /// </summary>
    GameEnded = GameStatusGroup | HasPayload | 0x5,

    /// <summary>
    /// The game status request packet type.
    /// </summary>
    GameStatusRequest = GameStatusGroup | 0x7,

#if DEBUG

    // GameState debug group (range: 0xC0 - 0xCF)

    /// <summary>
    /// The game state debug group packet type.
    /// </summary>
    GameStateDebugGroup = 0xC0,

#if !STEREO

    /// <summary>
    /// The set player score packet type.
    /// </summary>
    SetPlayerScore = GameStateDebugGroup | HasPayload | 0x1,

#endif

    // Debug group (range: 0xD0 - 0xDF)

    /// <summary>
    /// The debug group packet type.
    /// </summary>
    DebugGroup = 0xD0,

    /// <summary>
    /// The global ability use packet type.
    /// </summary>
    GlobalAbilityUse = DebugGroup | HasPayload | 0x3,

    /// <summary>
    /// The force end game packet type (debug).
    /// </summary>
    ForceEndGame = DebugGroup | 0x4,

#if STEREO

    /// <summary>
    /// The charge ability packet type.
    /// </summary>
    ChargeAbility = DebugGroup | HasPayload | 0x5,

#else

    /// <summary>
    /// The give ability packet type.
    /// </summary>
    GiveSecondaryItem = DebugGroup | HasPayload | 0x5,

    /// <summary>
    /// The global give ability packet type.
    /// </summary>
    GlobalGiveSecondaryItem = DebugGroup | HasPayload | 0x6,

#endif

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
    InvalidPacketTypeError = ErrorGroup | 0x1,

    /// <summary>
    /// The invalid packet type with payload.
    /// </summary>
    InvalidPacketTypeErrorWithPayload = InvalidPacketTypeError | HasPayload,

    /// <summary>
    /// The invalid packet usage.
    /// </summary>
    InvalidPacketUsageError = ErrorGroup | 0x2,

    /// <summary>
    /// The invalid packet usage with payload.
    /// </summary>
    InvalidPacketUsageErrorWithPayload = InvalidPacketUsageError | HasPayload,

    /// <summary>
    /// The invalid payload.
    /// </summary>
    InvalidPayloadError = ErrorGroup | 0x3,

    /// <summary>
    /// The invalid payload with payload.
    /// </summary>
    InvalidPayloadErrorWithPayload = InvalidPayloadError | HasPayload,

    /// <summary>
    /// The internal error.
    /// </summary>
    InternalError = ErrorGroup | 0x7,

    /// <summary>
    /// The internal error with payload.
    /// </summary>
    InternalErrorWithPayload = InternalError | HasPayload,
}
