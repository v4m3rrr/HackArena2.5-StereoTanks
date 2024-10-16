namespace GameClient;

/// <summary>
/// Reprsents a formatted localized string.
/// </summary>
/// <param name="Key">The key of the localized string.</param>
/// <param name="DefaultValue">
/// The default value to use if the localized string is not available.
/// </param>
/// <remarks>
/// This class is used to represent a localized string
/// that is formatted with a prefix and suffix.
/// </remarks>
internal record class FormattedLocalizedString(string Key, string? DefaultValue = null)
    : LocalizedString(Key, DefaultValue)
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
