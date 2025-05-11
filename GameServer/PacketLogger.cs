using System.Text;
using System.Text.Json;
using GameLogic.Networking;

namespace GameServer;

#if DEBUG

/// <summary>
/// Logs packets sent and received globally for debugging purposes (only in DEBUG).
/// </summary>
internal static class PacketLogger
{
    private static readonly object LockObj = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static StreamWriter? writer;

    /// <summary>
    /// Enables packet logging by opening a file for writing.
    /// </summary>
    /// <param name="logDirectory">Optional log directory. Defaults to 'packet_logs'.</param>
    public static void Enable(string? logDirectory = null)
    {
        if (writer is not null)
        {
            return;
        }

        logDirectory ??= "packet_logs";
        _ = Directory.CreateDirectory(logDirectory);

        string filename = $"packets-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        string fullPath = Path.Combine(logDirectory, filename);

        writer = new StreamWriter(fullPath, append: false, encoding: Encoding.UTF8)
        {
            AutoFlush = true,
        };
    }

    /// <summary>
    /// Logs a received packet.
    /// </summary>
    /// <param name="connection">The connection from which the packet was received.</param>
    /// <param name="packet">The received packet.</param>
    public static void LogReceived(Connection connection, Packet packet)
    {
        Log("RECEIVED", connection, packet);
    }

    /// <summary>
    /// Logs a sent packet.
    /// </summary>
    /// <param name="connection">The connection to which the packet was sent.</param>
    /// <param name="packet">The sent packet.</param>
    public static void LogSent(Connection connection, Packet packet)
    {
        Log("SENT", connection, packet);
    }

    private static void Log(string direction, Connection connection, Packet packet)
    {
        if (writer is null)
        {
            return;
        }

        var payloadElement = JsonDocument.Parse(packet.Payload?.ToString() ?? "{}").RootElement;

        var logEntry = new
        {
            Timestamp = DateTime.UtcNow.ToString("O"),
            Direction = direction,
            Connection = connection.ToString(),
            PacketType = packet.Type.ToString(),
            Payload = payloadElement,
        };

        string json = JsonSerializer.Serialize(logEntry, JsonOptions);

        lock (LockObj)
        {
            writer.WriteLine(json);
        }
    }
}

#endif
