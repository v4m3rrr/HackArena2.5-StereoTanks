using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the loading scene.
/// </summary>
internal class Loading : Scene, IOverlayScene
{
    private RoundedSolidColor background = default!;
    private ScalableTexture2D waitingIcon = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Loading"/> class.
    /// </summary>
    public Loading()
        : base(MonoTanks.ThemeColor)
    {
    }

    /// <inheritdoc/>
    public IEnumerable<IComponent> OverlayComponents => [this.BaseComponent];

    /// <inheritdoc/>
    public int Priority => ((Current as IOverlayScene)?.Priority + 1) ?? (1 << 28);

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.waitingIcon.IsEnabled)
        {
            this.waitingIcon.Rotation += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        MainEffect.Draw();
        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += (s, e) =>
        {
            (baseComponent as TextureComponent)!.Opacity = this.IsDisplayedOverlay ? 0.8f : 0f;
        };

        (baseComponent as TextureComponent)!.Opacity = 0.5f;

        this.background = new RoundedSolidColor(MonoTanks.ThemeColor, 25)
        {
            Parent = baseComponent,
            Opacity = 0.45f,
            Transform =
            {
                RelativeSize = new Vector2(0.32f, 0.19f),
                Alignment = Alignment.Center,
                RelativePadding = new Vector4(0.13f, 0.01f, 0.05f, 0.01f),
                MinSize = new Point(400, 120),
            },
        };

        this.waitingIcon = new ScalableTexture2D("Images/Icons/waiting.svg")
        {
            Parent = this.background,
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                RelativeSize = new Vector2(0.8f),
                Alignment = Alignment.Left,
                Ratio = new Ratio(1, 1),
            },
        };

        var container = new Container()
        {
            Parent = this.background,
            Transform =
            {
                RelativeSize = new Vector2(0.7f, 1f),
                Alignment = Alignment.Center,
            },
        };

        var font = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 24);

        var text = new LocalizedText(font, Color.White)
        {
            Parent = container,
            Spacing = 5,
            Case = TextCase.Upper,
            TextAlignment = Alignment.Center,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Value = new FormattedLocalizedString("Other.Loading", "Loading") { Suffix = "..." },
        };
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        this.background.Load();
        this.waitingIcon.Load();
    }
}
