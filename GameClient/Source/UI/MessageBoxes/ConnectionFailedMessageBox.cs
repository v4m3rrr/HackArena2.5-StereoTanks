using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI;

/// <summary>
/// Represents a message box that is displayed
/// when the connection to the server fails.
/// </summary>
internal class ConnectionFailedMessageBox : MessageBox<RoundedSolidColor>
{
    private readonly ScalableFont textFont = new("Content/Fonts/Orbitron-SemiBold.ttf", 9)
    {
        Spacing = 8,
    };

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

        var text = new LocalizedWrappedText(this.textFont, Color.DarkGray)
        {
            Value = reason,
        };

        this.SetTextProperties(text);
        this.CreateOkButton();
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

        var text = new WrappedText(this.textFont, Color.DarkGray)
        {
            Value = reason,
        };

        this.SetTextProperties(text);
        this.CreateOkButton();
    }

    private ConnectionFailedMessageBox()
        : base(new(new Color(120, 18, 12), 15))
    {
        this.CanBeClosedByClickOutside = false;

        this.Box.Transform.RelativeSize = new Vector2(0.31f, 0.19f);
        this.Box.Transform.MinSize = new Point(400, 245);
        this.Box.Transform.MaxSize = new Point(1000, 612);
        this.Box.Transform.Alignment = Alignment.Center;
        this.Box.Transform.RelativePadding = new Vector4(0.04f);
        this.Box.Load();

        this.Background = new SolidColor(MonoTanks.ThemeColor)
        {
            Opacity = 0.3f,
        };

        this.Background.Load();
    }

    private void SetTextProperties(WrappedText text)
    {
        text.Parent = this.Box;
        text.TextAlignment = Alignment.Center;
        text.Case = TextCase.Upper;
        text.Transform.RelativeSize = new Vector2(1.0f, 0.4f);
        text.Transform.RelativeOffset = new Vector2(0.0f, -0.05f);
        text.Transform.Alignment = Alignment.Center;
    }

    private void CreateTitle(string titleLocalizationKey)
    {
        var titleFont = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 18)
        {
            Spacing = 8,
        };

        // Title
        _ = new LocalizedText(titleFont, Color.White)
        {
            Parent = this.Box,
            Value = new LocalizedString(titleLocalizationKey),
            Case = TextCase.Upper,
            TextAlignment = Alignment.Center,
            TextShrink = TextShrinkMode.Width,
            Transform =
            {
                RelativeSize = new Vector2(1.0f, 0.25f),
                Alignment = Alignment.Top,
            },
        };
    }

    private void CreateOkButton()
    {
        var button = new Button<Container>(new Container())
        {
            Parent = this.Box,
            Transform =
            {
                RelativeSize = new Vector2(0.5f, 0.3f),
                Alignment = Alignment.Bottom,
            },
        };

        var buttonTextColor = Color.LightGray;
        var buttonTextHoveredColor = new Color(0xFF, 0xD2, 0x0);
        var buttonTextFont = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 14);
        var buttonText = new LocalizedText(buttonTextFont, buttonTextColor)
        {
            Parent = button.Component,
            Spacing = 5,
            Value = new FormattedLocalizedString("Buttons.Ok")
            {
                Prefix = "> ",
                Suffix = " <",
            },
            Case = TextCase.Upper,
            TextAlignment = Alignment.Center,
        };

        button.HoverEntered += (s, e) =>
        buttonText.Color = buttonTextHoveredColor;
        button.HoverExited += (s, e) => buttonText.Color = buttonTextColor;
        button.Clicked += (s, e) => ScreenController.HideOverlay(this);
    }
}
