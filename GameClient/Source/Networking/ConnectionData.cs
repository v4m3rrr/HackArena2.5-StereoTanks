using System;
using System.Collections.Generic;

namespace GameClient.Networking;

/// <summary>
/// Represents the connection data.
/// </summary>
internal readonly struct ConnectionData
{
    private ConnectionData(string serverAddress, bool isSpectator, string? joinCode, string? nickname)
    {
        this.ServerAddress = serverAddress;
        this.JoinCode = joinCode;
        this.IsSpectator = isSpectator;
        this.Nickname = nickname;
    }

#if DEBUG
    private ConnectionData(string serverAddress, bool isSpectator, string? joinCode, string? nickname, bool quickJoin)
        : this(serverAddress, isSpectator, joinCode, nickname)
    {
        this.QuickJoin = quickJoin;
    }
#endif

    /// <summary>
    /// Gets the address of the server to connect to.
    /// </summary>
    /// <value>
    /// The address of the server to connect to
    /// with the format "address:port".
    /// </value>
    public string ServerAddress { get; }

    /// <summary>
    /// Gets the join code to use when connecting to the server.
    /// </summary>
    public string? JoinCode { get; }

    /// <summary>
    /// Gets a value indicating whether the connection is for a spectator.
    /// </summary>
    public bool IsSpectator { get; }

    /// <summary>
    /// Gets the nickname of the player.
    /// </summary>
    public string? Nickname { get; }

#if DEBUG
    /// <summary>
    /// Gets a value indicating whether the player
    /// should join the game quickly.
    /// </summary>
    public bool QuickJoin { get; }
#endif

#pragma warning disable IDE0079
#pragma warning disable CS1572, SA1612
    /// <summary>
    /// Creates a new instance of the <see cref="ConnectionData"/> struct for a spectator.
    /// </summary>
    /// <param name="serverAddress">The address of the server to connect to.</param>
    /// <param name="joinCode">The join code to use when connecting to the server.</param>
    /// <param name="quickJoin">A value indicating whether the player should join the game quickly.</param>
    /// <returns>A new instance of the <see cref="ConnectionData"/> struct for a spectator.</returns>
#if DEBUG
    public static ConnectionData ForSpectator(string serverAddress, string? joinCode, bool quickJoin)
    {
        return new ConnectionData(serverAddress, true, joinCode, null, quickJoin);
    }
#else
    public static ConnectionData ForSpectator(string serverAddress, string? joinCode)
    {
        return new ConnectionData(serverAddress, true, joinCode, null);
    }
#endif

#pragma warning disable IDE0079
#pragma warning disable CS1572, SA1612
    /// <summary>
    /// Creates a new instance of the <see cref="ConnectionData"/> struct for a player.
    /// </summary>
    /// <param name="serverAddress">The address of the server to connect to.</param>
    /// <param name="joinCode">The join code to use when connecting to the server.</param>
    /// <param name="nickname">The nickname of the player.</param>
    /// <param name="quickJoin">A value indicating whether the player should join the game quickly.</param>
    /// <returns>A new instance of the <see cref="ConnectionData"/> struct for a player.</returns>
#pragma warning restore IDE0079, CS1572, SA1612
#if DEBUG
    public static ConnectionData ForPlayer(string serverAddress, string? joinCode, string nickname, bool quickJoin)
    {
        return new ConnectionData(serverAddress, false, joinCode, nickname, quickJoin);
    }
#else
    public static ConnectionData ForPlayer(string serverAddress, string? joinCode, string nickname)
    {
        return new ConnectionData(serverAddress, false, joinCode, nickname);
    }
#endif

    /// <summary>
    /// Returns the WebSocket URL of the server.
    /// </summary>
    /// <returns>The WebSocket URL of the server.</returns>
    public readonly string GetServerUrl()
    {
        var url = $"ws://{this.ServerAddress}";

        if (this.IsSpectator)
        {
            url += "/spectator";
        }

        var parameters = new List<string>();

        if (this.Nickname != null)
        {
            Debug.Assert(!this.IsSpectator, "Nickname is not allowed for spectators.");
            parameters.Add($"nickname={this.Nickname}");
        }

        if (this.JoinCode != null)
        {
            parameters.Add($"joinCode={this.JoinCode}");
        }

#if DEBUG
        if (this.QuickJoin)
        {
            parameters.Add("quickJoin=true");
        }
#endif

        if (parameters.Count > 0)
        {
            url += "?" + string.Join("&", parameters);
        }

        return url;
    }
}
