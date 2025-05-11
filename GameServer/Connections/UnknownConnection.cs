using System.Net;
using System.Net.WebSockets;
using GameLogic.Networking;
using Serilog;

namespace GameServer;

/// <summary>
/// Represents a connection with an unknown type.
/// </summary>
/// <param name="Context">The HTTP listener context.</param>
/// <param name="Socket">The WebSocket.</param>
/// <param name="EnumSerialization">The enum serialization format.</param>
/// <param name="Logger">The logger.</param>
internal record class UnknownConnection(
    HttpListenerContext Context,
    WebSocket Socket,
    EnumSerializationFormat EnumSerialization,
    ILogger Logger)
    : Connection(Context, Socket, new ConnectionData(EnumSerialization), Logger)
{
    /// <summary>
    /// Gets or sets the target type of the connection.
    /// </summary>
    public string TargetType { get; set; } = "Unknown";

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{base.ToString()}, Type=Unknown, TargetType={this.TargetType}";
    }
}
