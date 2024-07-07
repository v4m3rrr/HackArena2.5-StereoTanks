namespace GameClient;

/// <summary>
/// Represents a localized string.
/// </summary>
/// <param name="key">The key of the localized string.</param>
internal class LocalizedString(string key)
{
    /// <summary>
    /// Gets the default localized string.
    /// </summary>
    public static LocalizedString Default => new("No localization key specified.");

    /// <summary>
    /// Gets the localized string value.
    /// </summary>
    public string Value => Localization.Get(key);
}
