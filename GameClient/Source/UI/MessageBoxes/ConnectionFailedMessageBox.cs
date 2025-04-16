using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI;

/// <summary>
/// Represents a message box that is displayed
/// when the connection to the server fails.
/// </summary>
internal class ConnectionFailedMessageBox : MessageBox<RoundedSolidColor>
{
    private readonly ScalableFont textFont = new(Styles.Fonts.Paths.Main, 11)
    {
        AutoResize = true,
        Spacing = 8,
    };

    private readonly ScalableFont titleFont = new(Styles.Fonts.Paths.Main, 18)
    {
        AutoResize = true,
        Spacing = 8,
    };

    private readonly Container container;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionFailedMessageBox"/> class.
    /// </summary>
    /// <param name="reason">The reason why the connection failed.</param>
    public ConnectionFailedMessageBox(LocalizedString reason)
        : this("MessageBoxLabels.ConnectionFailed", reason)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionFailedMessageBox"/> class.
    /// </summary>
    /// <param name="reason">The reason why the connection failed.</param>
    public ConnectionFailedMessageBox(string reason)
        : this("MessageBoxLabels.ConnectionFailed", reason)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionFailedMessageBox"/> class.
    /// </summary>
    /// <param name="titleLocKey">The localization key of the title of the message box.</param>
    /// <param name="reason">The reason why the connection failed.</param>
    protected ConnectionFailedMessageBox(string titleLocKey, LocalizedString reason)
        : this()
    {
        this.CreateTitle(titleLocKey);

        var text = new LocalizedWrappedText(this.textFont, Color.White * 0.9f)
        {
            Value = reason,
        };

        this.SetTextProperties(text);
        this.CreateCloseButton();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionFailedMessageBox"/> class.
    /// </summary>
    /// <param name="titleLocKey">The localization key of the title of the message box.</param>
    /// <param name="reason">The reason why the connection failed.</param>
    protected ConnectionFailedMessageBox(string titleLocKey, string reason)
        : this()
    {
        this.CreateTitle(titleLocKey);

        var text = new WrappedText(this.textFont, Color.White * 0.9f)
        {
            Value = reason,
        };

        this.SetTextProperties(text);
        this.CreateCloseButton();
    }

    private ConnectionFailedMessageBox()
        : base(new((GameClientCore.ThemeColor * 0.35f).WithAlpha(255), 36) { AutoAdjustRadius = true })
    {
        this.CanBeClosedByClickOutside = false;

        this.Box.Transform.RelativeSize = new Vector2(0.43f, 0.205f);
        this.Box.Transform.Alignment = Alignment.Center;
        this.Box.Transform.RelativePadding = new Vector4(0.1f, 0.2f, 0.1f, 0.2f);
        this.Box.Load();

        this.container = new Container()
        {
            Parent = this.Box,
        };

        this.Background = new SolidColor(GameClientCore.ThemeColor)
        {
            Opacity = 0.3f,
        };

        this.Background.Load();
    }

    private void SetTextProperties(WrappedText text)
    {
        text.Parent = this.container;
        text.TextAlignment = Alignment.Bottom;
        text.Case = TextCase.Upper;
        text.Transform.RelativeSize = new Vector2(1.0f, 0.5f);
        text.Transform.Alignment = Alignment.Bottom;
        text.AdjustTransformSizeToText = AdjustSizeOption.OnlyHeight;
    }

    private void CreateTitle(string titleLocalizationKey)
    {
        _ = new LocalizedText(this.titleFont, Color.White)
        {
            Parent = this.container,
            Value = new LocalizedString(titleLocalizationKey),
            Case = TextCase.Upper,
            TextAlignment = Alignment.Center,
            TextShrink = TextShrinkMode.Width,
            Transform =
            {
                RelativeSize = new Vector2(1.0f, 0.5f),
                Alignment = Alignment.Top,
            },
        };
    }

    private void CreateCloseButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = this.Box,
            Transform =
            {
                Alignment = Alignment.TopRight,
                RelativeOffset = new Vector2(-0.02f, 0.07f),
                RelativeSize = new Vector2(0.15f),
                Ratio = new Ratio(1, 1),
                IgnoreParentPadding = true,
            },
        };

        var icon = new ScalableTexture2D("Images/Icons/exit.svg")
        {
            Parent = button.Component,
        };

        button.ApplyStyle(Styles.UI.ButtonStyle);
        button.GetDescendant<Text>()!.Scale = 0f; // Hide text
        button.Clicked += (s, e) => ScreenController.HideOverlay(this);
        icon.Load();
    }
}
