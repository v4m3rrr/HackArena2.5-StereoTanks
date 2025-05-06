namespace GameLogic.Networking;

/// <summary>
/// Represents a serialization context.
/// </summary>
/// <param name="enumSerialization">The enum serialization format.</param>
public class SerializationContext(EnumSerializationFormat enumSerialization)
{
    /// <summary>
    /// Gets the default serialization context.
    /// </summary>
    public static SerializationContext Default => new(SerializationOptions.Default.EnumSerialization);

    /// <summary>
    /// Gets the enum serialization format.
    /// </summary>
    public EnumSerializationFormat EnumSerialization { get; } = enumSerialization;
}
