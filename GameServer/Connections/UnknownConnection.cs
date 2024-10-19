using System.Net;
using System.Net.WebSockets;
using GameLogic.Networking;
using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents a connection with an unknown type.
/// </summary>
/// <param name="Context">The HTTP listener context.</param>
/// <param name="Socket">The WebSocket.</param>
/// <param name="EnumSerialization">The enum serialization format.</param>
/// <param name="Log">The logger.</param>
internal record class UnknownConnection(
    HttpListenerContext Context,
    WebSocket Socket,
    EnumSerializationFormat EnumSerialization,
    Logger Log)
    : Connection(Context, Socket, new ConnectionData(EnumSerialization), Log)
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
