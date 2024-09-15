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
        Action = (button) =>
        {
            var color = ButtonStyle!.GetProperty<Color>("UnhoveredColor")!;
            var offsetTextFromTexture = 60;

            var text = new LocalizedText(ButtonStyle!.GetPropertyOfType<ScalableFont>()!, color)
            {
                Parent = button.Component,
                Case = TextCase.Upper,
                TextAlignment = Alignment.Left,
            };

            var isRecalculating = false;
            text.Transform.Recalculating += (s, e) =>
            {
                offsetTextFromTexture = (int)(0.05f * ScreenController.Width);
                // Avoid infinite recursion
                if (!isRecalculating)
                {
                    isRecalculating = true;
                    text.Transform.SetRelativeOffsetFromAbsolute(x: offsetTextFromTexture);
                    isRecalculating = false;
                }
            };

            var texture = new ScalableTexture2D()
            {
                Parent = button.Component,
                Color = color,
                Transform =
                {
                    Ratio = new Ratio(1, 1),
                    RelativeSize = new Vector2(0.7f),
                    Alignment = Alignment.Left,
                },
            };

            void AdjustButtonSize()
            {
                var newWidth = text.Dimensions.X + offsetTextFromTexture + text.Font.Spacing;
                button.Transform.SetRelativeSizeFromAbsolute(x: newWidth);
            }

            text.ValueChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(texture.AssetPath))
                {
                    AdjustButtonSize();
                }
            };

            texture.Transform.SizeChanged += (s, e) =>
            {
                AdjustButtonSize();
            };
        },
        CustomProperties =
        [
            new Style.Property("Font", new ScalableFont("Content\\Fonts\\Orbitron-SemiBold.ttf", 12) { Spacing = 15 }),
            new Style.Property("HoveredColor", new Color(0xFF, 0xD2, 0x0)),
            new Style.Property("UnhoveredColor", Color.White),
        ],
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
            button.ApplyStyle(ButtonStyle);

            var backButtonTexture = button.Component.GetChild<ScalableTexture2D>()!;
            backButtonTexture.AssetPath = $"Images/back_icon.svg";
            backButtonTexture.Load();

            button.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Back");
        },
    };
}
