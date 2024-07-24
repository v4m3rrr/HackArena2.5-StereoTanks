namespace GameLogic;

/// <summary>
/// Provides utility methods for enums.
/// </summary>
internal static class EnumUtils
{
    private static readonly Random RandomGenerator = new();

    /// <summary>
    /// Returns a random value of the specified enum type.
    /// </summary>
    /// <typeparam name="T">The specified enum type.</typeparam>
    /// <returns>A random value of the specified enum type.</returns>
    public static T Random<T>()
        where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(RandomGenerator.Next(values.Length))!;
    }

    /// <summary>
    /// Returns the next value of the specified enum type.
    /// </summary>
    /// <typeparam name="T">The specified enum type.</typeparam>
    /// <param name="obj">The current value.</param>
    /// <returns>The next value of the specified enum type.</returns>
    public static T Next<T>(T obj)
        where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, obj);
        return (T)values.GetValue((index + 1) % values.Length)!;
    }

    /// <summary>
    /// Sets the specified enum value to the next value.
    /// </summary>
    /// <typeparam name="T">The specified enum type.</typeparam>
    /// <param name="obj">The current value.</param>
    public static void Next<T>(ref T obj)
        where T : struct, Enum
    {
        obj = Next(obj);
    }

    /// <summary>
    /// Returns the previous value of the specified enum type.
    /// </summary>
    /// <typeparam name="T">The specified enum type.</typeparam>
    /// <param name="obj">The current value.</param>
    /// <returns>The previous value of the specified enum type.</returns>
    public static T Previous<T>(T obj)
        where T : struct, Enum
    {
        var values = Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, obj);
        return (T)values.GetValue((index - 1 + values.Length) % values.Length)!;
    }

    /// <summary>
    /// Sets the specified enum value to the previous value.
    /// </summary>
    /// <typeparam name="T">The specified enum type.</typeparam>
    /// <param name="obj">The current value.</param>
    public static void Previous<T>(ref T obj)
        where T : struct, Enum
    {
        obj = Previous(obj);
    }
}
