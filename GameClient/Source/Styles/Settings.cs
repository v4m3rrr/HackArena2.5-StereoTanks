using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Styles;

/// <summary>
/// Represents the setting styles.
/// </summary>
internal static class Settings
{
    /// <summary>
    /// The font used in the setting elements.
    /// </summary>
    public static readonly ScalableFont Font = new("Content\\Fonts\\Consolas.ttf", 11);

    /// <summary>
    /// Gets the style of the frame label.
    /// </summary>
    public static Frame.Style FrameLabel { get; } = new()
    {
        Color = Color.Gray,
        Thickness = 1,
        Action = (Frame frame) =>
        {
            _ = new SolidColor(Color.White * 0.3f)
            {
                Parent = frame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };
            _ = new LocalizedText(Font, Color.White)
            {
                Parent = frame.InnerContainer,
                TextAlignment = Alignment.Left,
            };
        },
    };

    /// <summary>
    /// Gets the style of the frame value.
    /// </summary>
    public static Button<Frame>.Style SelectorItem { get; } = new()
    {
        HoverEntered = (s, e) =>
        {
            e.InnerContainer.GetChild<SolidColor>()!.Color = Color.White * 0.5f;
        },
        HoverExited = (s, e) =>
        {
            e.InnerContainer.GetChild<SolidColor>()!.Color = Color.White * 0.3f;
        },
        ComponentStyle = new Frame.Style()
        {
            Color = Color.Gray,
            Thickness = 1,
            Action = (Frame frame) =>
            {
                var background = new SolidColor(Color.White * 0.3f) { Parent = frame.InnerContainer };

                // Text cannot be added here because sometimes is localized
                // and cannot create a new instance that optionally supports localization (yet)
            },
        },
    };

    /// <summary>
    /// Gets the style of the selector active background.
    /// </summary>
    public static Frame.Style SelectorActiveBackground { get; } = new()
    {
        Color = Color.Gray,
        Thickness = 1,
        Action = (Frame frame) =>
        {
            _ = new SolidColor(new Color(40, 40, 40))
            {
                Parent = frame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };
        },
    };

    /// <summary>
    /// Gets the style of the selector inactive background.
    /// </summary>
    public static Frame.Style SelectorInactiveBackground { get; } = new()
    {
        Color = Color.Gray,
        Thickness = 1,
        Action = (Frame frame) =>
        {
            _ = new SolidColor(new Color(40, 40, 40))
            {
                Parent = frame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };

            // Text cannot be added here because sometimes is localized
            // and cannot create a new instance that optionally supports localization (yet)
        },
    };
}
