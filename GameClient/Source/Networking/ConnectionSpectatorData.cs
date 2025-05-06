namespace GameClient.Networking;

/// <summary>
/// Represents the connection data for a player.
/// </summary>
/// <param name="serverAddress">The address of the server to connect to.</param>
/// <param name="joinCode">The join code to use when connecting to the server.</param>
internal class ConnectionSpectatorData(string serverAddress, string? joinCode)
    : ConnectionData(serverAddress, joinCode, isSpectator: true)
{
}
