using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Styles;

/// <summary>
/// Represents the setting styles.
/// </summary>
internal static class SingleRoom
{
    /// <summary>
    /// The font used in the setting elements.
    /// </summary>
    private static readonly LocalizedScalableFont Font = new(14)
    {
        AutoResize = true,
        Spacing = 5,
    };

    /// <summary>
    /// Gets the style of the section input background.
    /// </summary>
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
                    RelativeSize = new Vector2(0.5f, 0.85f),
                    RelativePadding = new Vector4(0.05f, 0.01f, 0.05f, 0.01f),
                },
            };
        },
    };

    /// <summary>
    /// Gets the style for a section with a localized name.
    /// </summary>
    /// <param name="name">The localized name of the section.</param>
    /// <returns>The style for a section with a localized name.</returns>
    public static Style<Container> GetSectionStyle(LocalizedString name)
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
            },
        };
    }

    /// <summary>
    /// Gets the style for a section with a localized name and a text input.
    /// </summary>
    /// <param name="name">The localized name of the section.</param>
    /// <param name="charLimit">The character limit for the text input.</param>
    /// <returns>The style for a section with a localized name and a text input.</returns>
    public static Style<Container> GetSectionStyleWithTextInput(LocalizedString name, uint charLimit)
    {
        return new()
        {
            Action = (container) =>
            {
                var sectionStyle = GetSectionStyle(name);
                container.ApplyStyle(sectionStyle);

                var background = container.GetChild<SolidColor>();
                var input = new TextInput(Font, Color.White, Color.Gray)
                {
                    Parent = background,
                    TextAlignment = Alignment.Center,
                    TextShrink = TextShrinkMode.HeightAndWidth,
                    Case = TextCase.Upper,
                    ClearAfterSend = false,
                    AdjustCaretHeightToFont = true,
                    MaxLength = charLimit,
                };
            },
        };
    }
}
