using System.Net;
using System.Net.WebSockets;
using GameLogic;

namespace GameServer;

/// <summary>
/// Represents a connection to a player.
/// </summary>
/// <param name="Context">The HTTP listener context.</param>
/// <param name="Socket">The WebSocket.</param>
/// <param name="Instance">The player instance.</param>
/// <param name="Data">The connection data of the player.</param>
internal record class PlayerConnection(HttpListenerContext Context, WebSocket Socket, ConnectionData.Player Data, Player Instance)
    : Connection(Context, Socket, Data)
{
    /// <summary>
    /// Gets the player connection data.
    /// </summary>
    public new ConnectionData.Player Data { get; } = Data;

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
        return $"{base.ToString()}, Type=Player, Nickname={this.Instance.Nickname}";
    }
}
