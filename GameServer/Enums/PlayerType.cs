namespace GameServer;

/// <summary>
/// Represents a player type.
/// </summary>
public enum PlayerType
{
    /// <summary>
    /// The player is a human.
    /// </summary>
    Human,

#if HACKATON

    /// <summary>
    /// The player is a hackaton bot.
    /// </summary>
    HackatonBot,

#endif
}
