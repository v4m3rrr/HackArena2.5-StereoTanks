using System.Net;
using System.Net.WebSockets;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Routes incoming WebSocket connections to the appropriate connection type.
/// </summary>
internal sealed class ConnectionRoutingHandler(
    HttpListenerContext context,
    WebSocket socket,
    UnknownConnection unknownConnection,
    GameInstance game,
    ILogger logger)
{
    /// <summary>
    /// Performs routing based on the URL path.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RouteAsync()
    {
        string path = context.Request.Url?.AbsolutePath?.ToLowerInvariant() ?? string.Empty;

        switch (path)
        {
            case "/":
            case "":
                await new PlayerConnectionHandler(context, socket, unknownConnection, game, logger).HandleAsync();
                break;

            case "/spectator":
                await new SpectatorConnectionHandler(context, socket, unknownConnection, game, logger).HandleAsync();
                break;

            default:
                unknownConnection.TargetType = path[1..];
                await this.RejectAsync("InvalidUrlPath");
                break;
        }
    }

    private async Task RejectAsync(string reason)
    {
        logger.Information("Connection rejected; {connection}; {reason}", unknownConnection, reason);

        var payload = new ConnectionRejectedPayload(reason);
        var packet = new ResponsePacket(payload, logger);
        await packet.SendAsync(unknownConnection);
        await unknownConnection.CloseAsync(description: reason);
    }
}
