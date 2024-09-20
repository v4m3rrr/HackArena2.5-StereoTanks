using System.Net.WebSockets;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the lobby manager.
/// </summary>
/// <param name="game">The game instance.</param>
internal class LobbyManager(GameInstance game)
{
    /// <summary>
    /// Sends the lobby data to all players and spectators.
    /// </summary>
    public void SendLobbyDataToAll()
    {
        foreach (var player in game.PlayerManager.Players)
        {
            _ = this.SendLobbyDataToPlayer(player.Key, player.Value.Instance.Id);
        }

        foreach (var spectator in game.SpectatorManager.Spectators.Keys)
        {
            _ = this.SendLobbyDataToSpectator(spectator);
        }
    }

    /// <summary>
    /// Sends the lobby data to a player.
    /// </summary>
    /// <param name="player">The player to send the lobby data to.</param>
    /// <param name="playerId">The ID of the player.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendLobbyDataToPlayer(WebSocket player, string playerId)
    {
        var lobbyData = new LobbyDataPayload(
            playerId,
            [.. game.PlayerManager.Players.Values.Select(x => x.Instance)],
            game.Settings);

        var converters = LobbyDataPayload.GetConverters();
        await game.SendPlayerPacketAsync(player, lobbyData, converters);
    }

    /// <summary>
    /// Sends the lobby data to a spectator.
    /// </summary>
    /// <param name="spectator">The spectator to send the lobby data to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendLobbyDataToSpectator(WebSocket spectator)
    {
        var lobbyData = new LobbyDataPayload(
            PlayerId: null,
            [.. game.PlayerManager.Players.Values.Select(x => x.Instance)],
            game.Settings);

        var converters = LobbyDataPayload.GetConverters();
        await game.SendSpectatorPacketAsync(spectator, lobbyData, converters);
    }
}
