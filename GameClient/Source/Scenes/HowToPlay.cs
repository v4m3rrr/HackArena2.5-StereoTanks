using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the how to play scene.
/// </summary>
internal class HowToPlay : Scene
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HowToPlay"/> class.
    /// </summary>
    public HowToPlay()
        : base(Color.SaddleBrown)
    {
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        var font = new ScalableFont("Content\\Fonts\\Consolas.ttf", 11);

        //var backBtn = new Button<Frame>(new Frame())
        //{
        //    Parent = this.BaseComponent,
        //    Transform =
        //    {
        //        Alignment = Alignment.BottomLeft,
        //        RelativeOffset = new Vector2(0.04f, -0.04f),
        //        RelativeSize = new Vector2(0.12f, 0.07f),
        //    },
        //}.ApplyStyle(Styles.UI.ButtonStyle);
        //backBtn.Clicked += (s, e) => ChangeToPreviousOrDefault<MainMenu>();
        //backBtn.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Back");

        var frame = new Frame(Color.Black, 2)
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.Center,
                RelativeSize = new Vector2(0.95f, 0.8f),
                RelativeOffset = new Vector2(0.0f, -0.05f),
            },
        };
        var background = new SolidColor(Color.Black * 0.5f) { Parent = frame.InnerContainer };
        var listBox = new ScrollableListBox(new SolidColor(Color.Yellow))
        {
            Parent = frame.InnerContainer,
            Orientation = Orientation.Vertical,
            Spacing = 20,
            ScrollBar = { RelativeSize = 0.01f },
            //IsScrollable = true,
            //ScrollBar =
            //{
            //    FrameThickness = 1,
            //    FrameColor = Color.DarkGray * 0.75f,
            //    BackgroundColor = Color.Gray * 0.65f,
            //    ThumbColor = Color.Yellow,
            //    Parent = frame,
            //},
            //ContentContainerRelativeMargin = new Vector4(0.005f, 0.01f, 0.005f, 0.01f),
            //DrawContentOnMargin = true,
        };

        _ = new LocalizedWrappedText(font, Color.White)
        {
            Parent = listBox.ContentContainer,
            LineSpacing = 10,
            Value = new LocalizedString("Other.HowToPlay"),
            AdjustTransformSizeToText = AdjustSizeOption.OnlyHeight,
        };

        this.Showed += (s, e) =>
        {
            //listBox.ScrollBar?.ScrollTo(0.0f);
        };
    }
}
