using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents a localized scalable font.
/// </summary>
internal class LocalizedScalableFont : ScalableFont, ILocalizable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedScalableFont"/> class.
    /// </summary>
    /// <param name="size">The size of the font.</param>
    public LocalizedScalableFont(int size)
        : base(GetLocalizedFontPath(), size, GetSupportedCodePoints())
    {
        ILocalizable.AddReference(this);
    }

    /// <summary>
    /// Gets the native localized font based on the specified language.
    /// </summary>
    /// <param name="language">The language to get the localized font for.</param>
    /// <param name="size">The size of the font.</param>
    /// <param name="minSize">The minimum size of the font.</param>
    /// <param name="maxSize">The maximum size of the font.</param>
    /// <param name="spacing">The spacing between characters.</param>
    /// <param name="autoResize">Whether to automatically resize the font.</param>
    /// <returns>The native localized font.</returns>
    public static ScalableFont GetNativeLocalizedFont(
        Language language,
        int size,
        int minSize = 0,
        int maxSize = 0,
        float spacing = 0f,
        bool autoResize = false)
    {
        var fontPath = GetLocalizedFontPath(language);
        var (firstCodePoint, lastCodePoint) = GetSupportedCodePoints(language);
        return new ScalableFont(fontPath, size, firstCodePoint, lastCodePoint)
        {
            Spacing = spacing,
            AutoResize = autoResize,
            MinSize = minSize,
            MaxSize = maxSize,
        };
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        var fontPath = GetLocalizedFontPath();
        var (firstCodePoint, lastCodePoint) = GetSupportedCodePoints();
        this.Reload(fontPath, firstCodePoint, lastCodePoint);
    }

    private static string GetLocalizedFontPath(Language? language = null)
    {
        return (language ?? GameSettings.Language) switch
        {
            Language.English => Styles.Fonts.Paths.Main,
            Language.French => Styles.Fonts.Paths.Main,
            Language.Polish => "Content\\Fonts\\Exo2-SemiBold.ttf",
            Language.Russian => "Content\\Fonts\\Exo2-SemiBold.ttf",
            _ => Styles.Fonts.Paths.Main,
        };
    }

    private static (int First, int Last) GetSupportedCodePoints(Language? language = null)
    {
        return (language ?? GameSettings.Language) switch
        {
            Language.Russian => (0x20, 0x4FF),
            _ => (0x20, 0x4FF),
        };
    }
}
