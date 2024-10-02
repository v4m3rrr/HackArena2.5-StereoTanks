using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a serialization options.
/// </summary>
public class SerializationOptions
{
    /// <summary>
    /// Gets the default serialization options.
    /// </summary>
    public static SerializationOptions Default { get; } = new();

    /// <summary>
    /// Gets the formatting to use during serialization.
    /// </summary>
#if DEBUG
    public Formatting Formatting { get; init; } = Formatting.Indented;
#else
    public Formatting Formatting { get; init; } = Formatting.None;  
#endif

    /// <summary>
    /// Gets a value indicating whether to serialize the packet type as a string.
    /// </summary>
    /// <remarks>
    /// If <see langword="true"/>, the packet type will be serialized as a string.
    /// Otherwise, it will be serialized as an integer.
    /// </remarks>
    public TypeOfPacketType TypeOfPacketType { get; init; } = TypeOfPacketType.Int;
}
