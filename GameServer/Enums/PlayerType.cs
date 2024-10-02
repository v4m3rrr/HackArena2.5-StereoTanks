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

#if HACKATHON

    /// <summary>
    /// The player is a hackathon bot.
    /// </summary>
    HackathonBot,

#endif
}
