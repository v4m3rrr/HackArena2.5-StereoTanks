using System;

namespace GameClient.Networking;

/// <summary>
/// Represents the status of a connection.
/// </summary>
internal abstract record class ConnectionStatus
{
    /// <summary>
    /// Represents a successful connection.
    /// </summary>
    public record Success : ConnectionStatus;

    /// <summary>
    /// Represents a rejected connection.
    /// </summary>
    /// <param name="Reason">The reason for the rejection.</param>
    public record Rejected(string Reason) : ConnectionStatus;

    /// <summary>
    /// Represents a failed connection.
    /// </summary>
    /// <param name="Exception">The exception that caused the failure.</param>
    public record Failed(Exception? Exception = null) : ConnectionStatus;
}
