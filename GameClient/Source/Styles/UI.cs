using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Styles;

/// <summary>
/// Represents the UI styles.
/// </summary>
internal static class UI
{
    /// <summary>
    /// Gets the style of the button.
    /// </summary>
    public static IButton<Container>.Style ButtonStyle { get; } = new()
    {
        ["Font"] = new ScalableFont("Content\\Fonts\\Orbitron-SemiBold.ttf", 12) { Spacing = 15 },
        ["HoveredColor"] = new Color(0xFF, 0xD2, 0x0),
        ["UnhoveredColor"] = Color.White,
        Action = (button) =>
        {
            var color = ButtonStyle!.GetProperty<Color>("UnhoveredColor")!;

            var text = new LocalizedText(ButtonStyle!.GetPropertyOfType<ScalableFont>()!, color)
            {
                Parent = button.Component,
                Case = TextCase.Upper,
                TextAlignment = Alignment.Left,
            };
        },
        HoverEntered = (s, e) =>
        {
            var hoveredColor = ButtonStyle!.GetProperty<Color>("HoveredColor");
            e.GetAllDescendants<TextureComponent>().ToList().ForEach(texture =>
            {
                texture.Color = hoveredColor;
            });
            e.GetChild<LocalizedText>()!.Color = hoveredColor;
        },
        HoverExited = (s, e) =>
        {
            var unhoveredColor = ButtonStyle!.GetProperty<Color>("UnhoveredColor");
            e.GetAllDescendants<TextureComponent>().ToList().ForEach(texture =>
            {
                texture.Color = unhoveredColor;
            });
            e.GetChild<LocalizedText>()!.Color = unhoveredColor;
        },
    };

    /// <summary>
    /// Gets the style of the back button.
    /// </summary>
    public static IButton<Container>.Style BackButtonStyle { get; } = new()
    {
        Action = (button) =>
        {
            var style = GetButtonStyleWithIcon(new LocalizedString("Buttons.Back"), "Images/back_icon.svg", Alignment.Left);
            button.ApplyStyle(style);
        },
    };

    /// <summary>
    /// Gets the style of the button with an icon.
    /// </summary>
    /// <param name="text">The text on the button.</param>
    /// <param name="iconPath">The path to the icon.</param>
    /// <param name="iconAlignment">The alignment of the icon.</param>
    /// <returns>The style of the button with an icon.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the icon alignment does not have a left or right alignment flag.
    /// </exception>
    public static IButton<Container>.Style GetButtonStyleWithIcon(LocalizedString text, string iconPath, Alignment iconAlignment)
    {
        if (!(iconAlignment.HasFlag(Alignment.Right) || iconAlignment.HasFlag(Alignment.Left)))
        {
            throw new InvalidOperationException("Icon alignment must have a left or right alignment flag.");
        }

        return new()
        {
            Action = (button) =>
            {
                button.ApplyStyle(ButtonStyle);

                float offsetTextFromTexture = 0f;
                var color = ButtonStyle!.GetProperty<Color>("UnhoveredColor")!;
                var textComponent = button.GetDescendant<LocalizedText>()!;

                textComponent.TextAlignment = iconAlignment;

                var icon = new ScalableTexture2D()
                {
                    Parent = button.Component,
                    Color = color,
                    Transform =
                    {
                        Ratio = new Ratio(1, 1),
                        RelativeSize = new Vector2(0.7f),
                        Alignment = iconAlignment,
                    },
                };

                var isRecalculating = false;
                textComponent.Transform.Recalculating += (s, e) =>
                {
                    offsetTextFromTexture = (int)(0.05f * ScreenController.Width);

                    // Avoid infinite recursion
                    if (!isRecalculating)
                    {
                        isRecalculating = true;

                        if (iconAlignment.HasFlag(Alignment.Right))
                        {
                            offsetTextFromTexture *= -1;
                        }

                        textComponent.Transform.SetRelativeOffsetFromAbsolute(x: offsetTextFromTexture);
                        isRecalculating = false;
                    }
                };

                void AdjustButtonSize()
                {
                    var newWidth = textComponent.Dimensions.X + Math.Abs(offsetTextFromTexture) + textComponent.Font.Spacing;
                    button.Transform.SetRelativeSizeFromAbsolute(x: newWidth);
                }

                textComponent.ValueChanged += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(icon.AssetPath))
                    {
                        AdjustButtonSize();
                    }
                };

                icon.Transform.SizeChanged += (s, e) =>
                {
                    AdjustButtonSize();
                };

                var buttonTexture = button.Component.GetChild<ScalableTexture2D>()!;
                buttonTexture.AssetPath = iconPath;

                button.GetDescendant<LocalizedText>()!.Value = text;
            },
        };
    }
}
