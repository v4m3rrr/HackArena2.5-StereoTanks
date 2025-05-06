using GameLogic.Networking;

namespace GameClient.Networking;

/// <summary>
/// Represents the connection data.
/// </summary>
/// <param name="serverAddress">
/// The address of the server to connect to with the format "address:port".
/// </param>
/// <param name="joinCode">The join code to use when connecting to the server.</param>
/// <param name="isSpectator">
/// A value indicating whether the connection is for a spectator.
/// </param>
internal abstract class ConnectionData(string serverAddress, string? joinCode, bool isSpectator)
{
    /// <summary>
    /// Gets the address of the server to connect to.
    /// </summary>
    /// <value>
    /// The address of the server to connect to
    /// with the format "address:port".
    /// </value>
    public string ServerAddress { get; } = serverAddress;

    /// <summary>
    /// Gets the join code to use when connecting to the server.
    /// </summary>
    public string? JoinCode { get; } = joinCode;

    /// <summary>
    /// Gets a value indicating whether the connection is for a spectator.
    /// </summary>
    public bool IsSpectator { get; } = isSpectator;

#if DEBUG

    /// <summary>
    /// Gets a value indicating whether the player
    /// should join the game quickly.
    /// </summary>
    public bool QuickJoin { get; init; }

#endif

    /// <summary>
    /// Returns the WebSocket URL of the server.
    /// </summary>
    /// <returns>The WebSocket URL of the server.</returns>
    public string GetServerUrl()
    {
        var url = $"ws://{this.ServerAddress}";

        if (this.IsSpectator)
        {
            url += "/spectator";
        }

        var parameters = this.GetUrlParameters();

        if (parameters.Count > 0)
        {
            url += "?" + string.Join("&", parameters);
        }

        return url;
    }

    /// <summary>
    /// Gets the URL parameters to be used in the WebSocket URL.
    /// </summary>
    /// <returns>A list of URL parameters to be used in the WebSocket URL.</returns>
    protected virtual List<string> GetUrlParameters()
    {
        List<string> parameters = [];

        if (this.JoinCode != null)
        {
            parameters.Add($"joinCode={this.JoinCode}");
        }

        if (!this.IsSpectator)
        {
            parameters.Add("playerType=human");
        }

        if (SerializationContext.Default.EnumSerialization is EnumSerializationFormat.String)
        {
            parameters.Add("enumSerializationFormat=string");
        }

#if DEBUG
        if (this.QuickJoin)
        {
            parameters.Add("quickJoin=true");
        }
#endif

        return parameters;
    }
}
