using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents an error payload.
/// </summary>
/// <remarks>
/// <para>This class is used to send error messages to clients.</para>
/// <para>The error message is sent as a string.</para>
/// <para>The packet type must be an error group type.</para>
/// </remarks>
public class ErrorPayload : IPacketPayload
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorPayload"/> class.
    /// </summary>
    /// <param name="type">The packet type from the error group.</param>
    /// <param name="message">The message to send.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the packet type is not an error group type.
    /// </exception>
    public ErrorPayload(PacketType type, string? message = null)
    {
        if (!type.HasFlag(PacketType.ErrorGroup))
        {
            throw new ArgumentException("The packet type must be an error group type.", nameof(type));
        }

        this.Type = this.ErrorType = type;
        this.Message = message ?? string.Empty;
    }

    [JsonConstructor]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "To avoid another constructor with the same signature.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0051:Remove unused private members",
        Justification = "Used for deserialization.")]
    private ErrorPayload(PacketType type, PacketType errorType, string? message)
    {
        this.Type = this.ErrorType = errorType;
        this.Message = message ?? string.Empty;
    }

    /// <inheritdoc/>
    public PacketType Type { get; }

    /// <summary>
    /// Gets the message.
    /// </summary>
    public string Message { get; }

    [JsonProperty]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0052:Remove unread private members",
        Justification = "Used for Type serialization.")]
    private PacketType ErrorType { get; }
}
