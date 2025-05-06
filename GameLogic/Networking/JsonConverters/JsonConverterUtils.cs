using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Provides utilities for JSON converters.
/// </summary>
public static class JsonConverterUtils
{
    /// <summary>
    /// Serializes the specified enum value with the specified format.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="value">The value of the enum to serialize.</param>
    /// <param name="format">The format to serialize the enum value as.</param>
    /// <returns>The serialized enum value.</returns>
    public static JToken WriteEnum<T>(T value, EnumSerializationFormat format)
        where T : notnull, Enum
    {
        if (format == EnumSerializationFormat.Int)
        {
            return Convert.ToInt32(value);
        }

        if (format == EnumSerializationFormat.String)
        {
            var str = value.ToString();
            return char.ToLowerInvariant(str[0]) + str[1..];
        }

        throw new ArgumentOutOfRangeException(nameof(format), format, null);
    }

    /// <summary>
    /// Deserializes the specified enum value with the specified format.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="token">The token to deserialize the enum value from.</param>
    /// <param name="format">The format to deserialize the enum value as.</param>
    /// <returns>The deserialized enum value.</returns>
    public static T ReadEnum<T>(JToken token, EnumSerializationFormat format)
        where T : notnull, Enum
    {
        return format switch
        {
            EnumSerializationFormat.Int => (T)Enum.ToObject(typeof(T), token.Value<int>()),
            EnumSerializationFormat.String => (T)Enum.Parse(typeof(T), token.Value<string>()!, ignoreCase: true),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
    }

    /// <summary>
    /// Deserializes the specified enum value with an inferred format.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="token">The token to deserialize the enum value from.</param>
    /// <returns>The deserialized enum value.</returns>
    public static T ReadEnum<T>(JToken token)
        where T : notnull, Enum
    {
        return token.Type switch
        {
            JTokenType.Integer => ReadEnum<T>(token, EnumSerializationFormat.Int),
            JTokenType.String => ReadEnum<T>(token, EnumSerializationFormat.String),
            _ => throw new ArgumentOutOfRangeException(nameof(token), token.Type, null),
        };
    }
}
