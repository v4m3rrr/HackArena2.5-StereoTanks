using System.Net;
using System.Net.WebSockets;
using GameLogic.Networking;
using GameServer.System;
using Serilog;

namespace GameServer;

/// <summary>
/// Handles incoming spectator connection.
/// </summary>
/// <param name="context">The HTTP listener context.</param>
/// <param name="socket">The WebSocket connection.</param>
/// <param name="unknown">The unknown connection.</param>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger.</param>
internal sealed class SpectatorConnectionHandler(
    HttpListenerContext context,
    WebSocket socket,
    UnknownConnection unknown,
    GameInstance game,
    ILogger logger)
{
    /// <summary>
    /// Handles the spectator connection request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync()
    {
        unknown.TargetType = "Spectator";

#if DEBUG
        _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#else
        const bool quickJoin = false;
#endif

        var connectionData = new ConnectionData(unknown.EnumSerialization)
        {
#if DEBUG
            QuickJoin = quickJoin,
#endif
        };

        var connection = new SpectatorConnection(context, socket, connectionData, logger);
        await this.AcceptAsync(connection);
        game.AddConnection(connection);

        _ = Task.Run(() => PacketListeningService.StartReceivingAsync(game, connection, game.PacketHandler, logger));
        _ = Task.Run(() => PingHelper.Start(connection, game, logger));

        await game.LobbyManager.SendLobbyDataTo(connection);

        if (quickJoin)
        {
            game.GameManager.StartGame();
        }
        else if (game.GameManager.IsInProgess)
        {
            await game.LobbyManager.SendGameStartedTo(connection);
        }
    }

    private async Task AcceptAsync(Connection connection)
    {
        var payload = new EmptyPayload { Type = PacketType.ConnectionAccepted };
        var packet = new ResponsePacket(payload, logger);
        await packet.SendAsync(connection);
    }
}
