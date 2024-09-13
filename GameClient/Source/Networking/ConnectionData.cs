using System;

namespace GameClient.Networking;

/// <summary>
/// Represents the connection data.
/// </summary>
/// <param name="ServerAddress">The address of the server to connect to.</param>
/// <param name="Port">The port of the server to connect to.</param>
/// <param name="IsSpectator">A value indicating whether the client is a spectator.</param>
/// <param name="JoinCode">The join code to use when connecting to the server.</param>
internal record struct ConnectionData(
    string ServerAddress,
    int Port,
    bool IsSpectator,
    string? JoinCode)
{
    /// <summary>
    /// Returns the WebSocket URL of the server.
    /// </summary>
    /// <returns>The WebSocket URL of the server.</returns>
    public readonly string GetServerWsUrl()
    {
        var url = $"ws://{this.ServerAddress}:{this.Port}";

        if (this.IsSpectator)
        {
            url += "/spectator";
        }

        if (this.JoinCode != null)
        {
            url += $"?joinCode={this.JoinCode}";
        }

        return url;
    }

    /// <summary>
    /// Returns the HTTP URL of the server.
    /// </summary>
    /// <returns>The HTTP URL of the server.</returns>
    public readonly string GetServerHttpUrl()
    {
        return string.Concat("http", this.GetServerWsUrl().AsSpan(2));
    }
}
