using System.Net;
using System.Net.WebSockets;
using System.Text;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Handles incoming HTTP WebSocket connections.
/// </summary>
/// <param name="context">The HTTP listener context for the connection.</param>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger.</param>
internal sealed class HttpConnectionHandler(HttpListenerContext context, GameInstance game, ILogger logger)
{
    /// <summary>
    /// Handles the incoming request asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync()
    {
        if (!context.Request.IsWebSocketRequest)
        {
            await this.RejectWithMessageAsync("WebSocket is required");
            return;
        }

        WebSocket socket;
        try
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            socket = wsContext.WebSocket;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to accept WebSocket connection.");
            return;
        }

        EnumSerializationFormat format = this.ParseEnumFormat();
        var connection = new UnknownConnection(context, socket, format, logger);

        var handler = new ConnectionRoutingHandler(context, socket, connection, game, logger);
        await handler.RouteAsync();
    }

    private EnumSerializationFormat ParseEnumFormat()
    {
        string? rawFormat = context.Request.QueryString["enumSerializationFormat"];
        return Enum.TryParse(rawFormat, ignoreCase: true, out EnumSerializationFormat format)
            ? format
            : EnumSerializationFormat.Int;
    }

    private async Task RejectWithMessageAsync(string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
    }
}
