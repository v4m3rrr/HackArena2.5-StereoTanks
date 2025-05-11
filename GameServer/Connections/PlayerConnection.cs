using System.Net;
using System.Net.WebSockets;
using GameLogic;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Represents a connection to a player.
/// </summary>
/// <param name="Context">The HTTP listener context.</param>
/// <param name="Socket">The WebSocket.</param>
/// <param name="Data">The connection data of the player.</param>
/// <param name="Logger">The logger.</param>
/// <param name="Instance">The player instance.</param>
internal record class PlayerConnection(
    HttpListenerContext Context,
    WebSocket Socket,
    ConnectionData.Player Data,
    ILogger Logger,
    Player Instance)
    : Connection(Context, Socket, Data, Logger)
{
#if STEREO

    /// <summary>
    /// Gets the team of the player.
    /// </summary>
    public required Team Team { get; init; }

#endif

    /// <summary>
    /// Gets the player connection data.
    /// </summary>
    public new ConnectionData.Player Data { get; } = Data;

    /// <summary>
    /// Gets the player identifier.
    /// </summary>
    /// <remarks>
    /// This is primarily used for logging purposes.
    /// </remarks>
    public string Identifier
#if STEREO
        => $"{this.Team.Name}/{this.Instance.Tank.Type}";
#else
        => this.Instance.Nickname;
#endif

    /// <summary>
    /// Gets or sets a value indicating whether the player
    /// has responded with a move in this game tick.
    /// </summary>
    /// <remarks>
    /// It is used to ensure that a player can only make one move per tick.
    /// </remarks>
    public bool HasMadeActionThisTick { get; set; }

#if HACKATHON

    /// <summary>
    /// Gets a value indicating whether the player is a bot.
    /// </summary>
    public bool IsHackathonBot => this.Data.Type == PlayerType.HackathonBot;

    /// <summary>
    /// Gets or sets a value indicating whether the player
    /// has made an action to the current game state.
    /// </summary>
    public bool HasMadeActionToCurrentGameState { get; set; }

#endif

#if HACKATHON && STEREO

    /// <summary>
    /// Gets or sets the last sent game state payload.
    /// </summary>
    public GameStatePayload.ForPlayer? LastGameStatePayload { get; set; }

#endif

    /// <summary>
    /// Resets the game tick properties of the player.
    /// </summary>
    public void ResetGameTickProperties()
    {
        lock (this)
        {
            this.HasMadeActionThisTick = false;
#if HACKATHON
            this.HasMadeActionToCurrentGameState = false;
#endif
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return
#if HACKATHON
            $"{base.ToString()}, Type={this.Data.Type}";
#elif STEREO
            $"{base.ToString()}, TeamName={this.Instance.Team.Name}, TankType={this.Instance.Tank.Type}";
#else
            $"{base.ToString()}, Nickname={this.Instance.Nickname}";
#endif
    }
}
