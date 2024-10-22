using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Styles;

/// <summary>
/// Represents the setting styles.
/// </summary>
internal static class JoinRoom
{
    /// <summary>
    /// The font used in the setting elements.
    /// </summary>
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 14)
    {
        AutoResize = true,
        Spacing = 5,
    };

    /// <summary>
    /// Gets the style for a section.
    /// </summary>
    /// <remarks>
    /// This style does not include the name of the section.
    /// </remarks>
    public static Style<Container> SectionStyle { get; } = new()
    {
        Action = (container) =>
        {
            var inputBackground = new RoundedSolidColor(Color.White, 100)
            {
                Parent = container,
                Opacity = 0.45f,
                Transform =
                {
                    Alignment = Alignment.Right,
                    RelativeSize = new Vector2(0.5f, 1.0f),
                    RelativePadding = new Vector4(0.05f, 0.01f, 0.05f, 0.01f),
                },
            };

            var input = new TextInput(Font, Color.White, Color.Gray)
            {
                Parent = inputBackground,
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.HeightAndWidth,
                Case = TextCase.Upper,
                ClearAfterSend = false,
                AdjustCaretHeightToFont = true,
            };
        },
    };

    /// <summary>
    /// Gets the style for a section with a localized name and a character limit.
    /// </summary>
    /// <param name="name">The localized name of the section.</param>
    /// <param name="charLimit">The character limit for the text input.</param>
    /// <returns>The style for a section with a localized name and a character limit.</returns>
    public static Style<Container> GetSectionStyleWithLocalizedName(LocalizedString name, uint charLimit)
    {
        return new()
        {
            Action = (container) =>
            {
                _ = new LocalizedText(Font, Color.White)
                {
                    Parent = container,
                    Value = name,
                    Case = TextCase.Upper,
                    TextAlignment = Alignment.Left,
                    TextShrink = TextShrinkMode.HeightAndWidth,
                    Transform =
                    {
                        RelativeSize = new Vector2(0.5f, 1.0f),
                    },
                };

                container.ApplyStyle(SectionStyle);
                var textInput = container.GetDescendant<TextInput>()!;
                textInput.MaxLength = charLimit;
            },
        };
    }
}
