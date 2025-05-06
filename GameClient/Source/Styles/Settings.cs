using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Styles;

/// <summary>
/// Represents the setting styles.
/// </summary>
internal static class Settings
{
    private static readonly ScalableFont Font = new(Fonts.Paths.Main, 13)
    {
        AutoResize = true,
        Spacing = 5,
        MinSize = 6,
    };

    private static readonly LocalizedScalableFont LocalizedFont = new(13)
    {
        AutoResize = true,
        Spacing = 5,
        MinSize = 6,
    };

    /// <summary>
    /// Gets the style of the selector scroll bar.
    /// </summary>
    public static Style<ScrollBar> SelectorScrollBar { get; } = new()
    {
        Action = (scrollBar) =>
        {
            var thumb = new RoundedSolidColor(Color.White * 0.8f, 5)
            {
                AutoAdjustRadius = true,
            };

            scrollBar.Thumb = thumb;
            scrollBar.RelativeSize = 0.036f;
            scrollBar.IsPriority = true;
            scrollBar.Transform.RelativeOffset = new Vector2(0.07f, 0.0f);
            scrollBar.Transform.IgnoreParentPadding = true;
        },
    };

    /// <summary>
    /// Gets the style of the selector.
    /// </summary>
    /// <param name="localized">
    /// A value indicating whether the text should be localized.
    /// </param>
    /// <returns>The style of the selector.</returns>
    public static Style<ISelector> GetSelectorStyle(bool localized = false)
    {
        return new()
        {
            Action = (selector) =>
            {
                var activeColor = (GameClientCore.ThemeColor * 0.3f).WithAlpha(0xFF);
                var activeBackground = new RoundedSolidColor(activeColor, 40)
                {
                    Parent = selector.ActiveContainer,
                    AutoAdjustRadius = true,
                };

                var inactiveBackground = new RoundedSolidColor(Color.White, 40)
                {
                    Parent = selector.InactiveContainer,
                    AutoAdjustRadius = true,
                    Opacity = 0.35f,
                };

                // Info about the selected item (inactive)
                Text text = localized
                    ? new LocalizedText(LocalizedFont, Color.White)
                    : new Text(Font, Color.White);

                text.Parent = selector.InactiveContainer;
                text.TextAlignment = Alignment.Center;
                text.TextShrink = TextShrinkMode.Width;
                text.Case = TextCase.Upper;

                var listBox = selector.ListBox;
                listBox.Orientation = Orientation.Vertical;
                listBox.Spacing = 10;
                listBox.Transform.RelativePadding = new Vector4(0.05f, 0.015f, 0.05f, 0.015f);

                selector.ItemSelected += (s, e) =>
                {
                    foreach (var item in selector.Items)
                    {
                        item.Button.Component.GetChild<Text>()!.Color = Color.White;
                    }

                    if (e is not null)
                    {
                        e.Button.Component.GetChild<Text>()!.Color = GameClientCore.ThemeColor;
                    }
                };

                if (listBox is ScrollableListBox scrollableListBox)
                {
                    scrollableListBox.ScrollBar.ApplyStyle(SelectorScrollBar);
                    scrollableListBox.DrawContentOnParentPadding = true;
                }
            },
        };
    }

    /// <summary>
    /// Gets the style of the selector button item.
    /// </summary>
    /// <param name="localized">
    /// A value indicating whether the text should be localized.
    /// </param>
    /// <returns>The style of the selector button item.</returns>
    public static IButton<Container>.Style GetSelectorButtonItem(bool localized = false)
    {
        return new()
        {
            ["FontColor"] = Color.White,
            Action = (button) =>
            {
                button.Component.Transform.RelativePadding = new(0.05f, 0.1f, 0.05f, 0.1f);

                var background = new RoundedSolidColor(Color.Transparent, 40)
                {
                    Parent = button.Component,
                    AutoAdjustRadius = true,
                    Transform = { IgnoreParentPadding = true },
                };

                var hoverEffect = new RoundedSolidColor(Color.Transparent, 40)
                {
                    Parent = background,
                    AutoAdjustRadius = true,
                    Transform = { IgnoreParentPadding = true },
                };

                Text text = localized
                    ? new LocalizedText(LocalizedFont, Color.White)
                    : new Text(Font, Color.White);

                text.Parent = button.Component;
                text.TextAlignment = Alignment.Center;
                text.TextShrink = TextShrinkMode.Width;
            },
            HoverEntered = (s, e) =>
            {
                var background = e.GetChild<RoundedSolidColor>()!;
                background.GetChild<RoundedSolidColor>()!.Color = Color.White * 0.5f;
            },
            HoverExited = (s, e) =>
            {
                var background = e.GetChild<RoundedSolidColor>()!;
                background.GetChild<RoundedSolidColor>()!.Color = Color.Transparent;
            },
        };
    }
}
