namespace GameLogic.Networking.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an enum conversion fails.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
/// <param name="value">The value that failed to convert to the enum type.</param>
internal class PayloadEnumValidationError<T>(string value)
    : Exception($"Failed to convert '{value}' to '{typeof(T).Name}'.")
    where T : notnull, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadEnumValidationError{T}"/> class.
    /// </summary>
    /// <param name="value">The value that failed to convert to the enum type.</param>
    public PayloadEnumValidationError(T value)
        : this(value.ToString()!)
    {
    }
}
