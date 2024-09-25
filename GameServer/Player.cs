using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents a player connected to the server.
/// </summary>
public class Player(GameLogic.Player instance, PlayerConnectionData connectionData)
{
    private readonly object pingReceivedLock = new();

    private bool hasSentPong;

    /// <summary>
    /// Gets the player game logic instance.
    /// </summary>
    public GameLogic.Player Instance { get; } = instance;

    /// <summary>
    /// Gets the player connection data.
    /// </summary>
    public PlayerConnectionData ConnectionData { get; } = connectionData;

    /// <summary>
    /// Gets or sets the time when the last ping was sent.
    /// </summary>
    public DateTime LastPingSentTime { get; set; }

#if HACKATON

    /// <summary>
    /// Gets a value indicating whether the player is a bot.
    /// </summary>
    public bool IsHackatonBot => this.ConnectionData.Type == PlayerType.HackatonBot;

#endif

    /// <summary>
    /// Gets or sets a value indicating whether a player has sent a pong.
    /// </summary>
    /// <remarks>
    /// This property is thread-safe.
    /// </remarks>
    public bool HasSentPong
    {
        get
        {
            lock (this.pingReceivedLock)
            {
                return this.hasSentPong;
            }
        }

        set
        {
            lock (this.pingReceivedLock)
            {
                this.hasSentPong = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the player
    /// has responded with a move in this game tick.
    /// </summary>
    /// <remarks>
    /// It is used to ensure that a player can only make one move per tick.
    /// </remarks>
    public bool HasMadeActionThisTick { get; set; }

#if HACKATON

    /// <summary>
    /// Gets or sets a value indicating whether the player
    /// has made an action to the current game state.
    /// </summary>
    public bool HasMadeActionToCurrentGameState { get; set; }

#endif
}
