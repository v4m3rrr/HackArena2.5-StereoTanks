namespace GameLogic.Networking.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an enum conversion fails.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
/// <param name="value">The value that failed to convert to the enum.</param>
internal class ConvertEnumFailed<T>(string value)
    : Exception($"Failed to convert '{value}' to '{typeof(T).Name}'.")
    where T : notnull, Enum
{
}
