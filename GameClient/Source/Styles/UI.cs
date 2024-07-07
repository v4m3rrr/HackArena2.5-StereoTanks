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
    public static Button<Frame>.Style ButtonStyle { get; } = new()
    {
        HoverEntered = (s, e) =>
        {
            e.Thickness = ButtonStyle!.GetProperty<int>("FrameThicknessHovered");
            e.Color = ButtonStyle.GetProperty<Color>("FrameColorHovered");
        },
        HoverExited = (s, e) =>
        {
            e.Thickness = ButtonStyle!.GetProperty<int>("FrameThicknessUnhovered");
            e.Color = ButtonStyle.GetProperty<Color>("FrameColorUnhovered");
        },
        CustomProperties =
        [
            new Style.Property("Font", new ScalableFont("Content\\Fonts\\Tiny5-Regular.ttf", 15)),
            new Style.Property("FrameColorHovered", new Color(247, 103, 7, 255)),
            new Style.Property("FrameColorUnhovered", new Color(66, 94, 201, 210)),
            new Style.Property("FrameThicknessHovered", 4),
            new Style.Property("FrameThicknessUnhovered", 2),
        ],
        ComponentStyle = new Frame.Style()
        {
            Action = (Frame x) =>
            {
                x.Thickness = ButtonStyle!.GetProperty<int>("FrameThicknessUnhovered");
                x.Color = ButtonStyle.GetProperty<Color>("FrameColorUnhovered");

                var background = new SolidColor(Color.Blue) { Parent = x.InnerContainer };
                var text = new LocalizedText(ButtonStyle.GetPropertyOfType<ScalableFont>()!, Color.White)
                {
                    Parent = x.Parent,
                    TextAlignment = Alignment.Center,
                    TextShrink = TextShrinkMode.Width,
                    Case = TextCase.Upper,
                    Transform =
                    {
                        RelativeSize = new Vector2(0.9f, 0.95f),
                        RelativeOffset = new Vector2(0.0f, 0.075f),
                        Alignment = Alignment.Center,
                    },
                };
            },
        },
    };
}