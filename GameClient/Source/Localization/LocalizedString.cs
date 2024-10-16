namespace GameClient;

/// <summary>
/// Represents a localized string.
/// </summary>
/// <param name="Key">The key of the localized string.</param>
/// <param name="DefaultValue">The default value to use if the localized string is not available.</param>
/// <remarks>
/// <para>
/// If the localized string is not available, the default value will be used.
/// </para>
/// <para>
/// If the default value is not specified, the key will be used as the default value.
/// </para>
/// </remarks>
internal record class LocalizedString(string Key, string? DefaultValue = null)
{
    /// <summary>
    /// Gets the default localized string.
    /// </summary>
    public static LocalizedString Default => new("No localization key specified.");

    /// <summary>
    /// Gets the localized string value.
    /// </summary>
    /// <value>
    /// The localized string value if it is available;
    /// <see cref="DefaultValue"/> if value is not available,
    /// but <see cref="DefaultValue"/> is specified;
    /// otherwise, <see cref="Key"/>.
    /// </value>
    public virtual string Value => Localization.Get(this.Key) ?? this.DefaultValue ?? this.Key;
}
