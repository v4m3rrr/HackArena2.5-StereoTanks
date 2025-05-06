using GameLogic;

namespace GameClient.Networking;

/// <summary>
/// Represents the connection data for a player.
/// </summary>
/// <param name="serverAddress">The address of the server to connect to.</param>
/// <param name="joinCode">The join code to use when connecting to the server.</param>
internal class ConnectionPlayerData(string serverAddress, string? joinCode)
    : ConnectionData(serverAddress, joinCode, isSpectator: false)
{
#if STEREO

    /// <summary>
    /// Gets the name of the team.
    /// </summary>
    public required string TeamName { get; init; }

    /// <summary>
    /// Gets the type of the tank.
    /// </summary>
    public required TankType TankType { get; init; }

#else

    /// <summary>
    /// Gets the nickname of the player.
    /// </summary>
    public required string Nickname { get; init; }

#endif

    /// <inheritdoc/>
    protected override List<string> GetUrlParameters()
    {
        var parameters = base.GetUrlParameters();

#if STEREO
        parameters.Add($"tankType={this.TankType}");
        parameters.Add($"teamName={this.TeamName}");
#else

        parameters.Add($"nickname={this.Nickname}");
#endif
        return parameters;
    }
}
