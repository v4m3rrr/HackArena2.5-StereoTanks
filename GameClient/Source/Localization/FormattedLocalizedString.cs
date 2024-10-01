namespace GameClient;

/// <summary>
/// Reprsents a formatted localized string.
/// </summary>
/// <param name="key">The key of the localized string.</param>
/// <remarks>
/// This class is used to represent a localized string
/// that is formatted with a prefix and suffix.
/// </remarks>
internal class FormattedLocalizedString(string key) : LocalizedString(key)
{
    /// <summary>
    /// Gets or sets the prefix of the localized string.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the suffix of the localized string.
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    /// Gets the formatted localized string value.
    /// </summary>
    public override string Value => $"{this.Prefix}{base.Value}{this.Suffix}";
}
