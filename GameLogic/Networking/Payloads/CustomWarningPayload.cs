namespace GameLogic.Networking;

/// <summary>
/// Represents a custom warning payload.
/// </summary>
/// <param name="message">The warning message.</param>
public class CustomWarningPayload(string message) : IPacketPayload
{
    /// <summary>
    /// Gets the packet type.
    /// </summary>
    public PacketType Type => PacketType.CustomWarning;

    /// <summary>
    /// Gets the warning message.
    /// </summary>
    public string Message { get; } = message;
}
