using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI;

/// <summary>
/// Represents a message box that shows a connecting message.
/// </summary>
internal class ConnectingMessageBox : MessageBox<RoundedSolidColor>
{
    private readonly WrappedText text;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectingMessageBox"/> class.
    /// </summary>
    /// <param name="cancellationTokenSource">The cancellation token source to cancel the connection.</param>
    public ConnectingMessageBox(CancellationTokenSource? cancellationTokenSource = null)
        : base(new((GameClientCore.ThemeColor * 0.35f).WithAlpha(255), 36) { AutoAdjustRadius = true })
    {
        this.CanBeClosedByClickOutside = false;

        this.Box.Transform.RelativeSize = new Vector2(0.32f, 0.19f);
        this.Box.Transform.Alignment = Alignment.Center;
        this.Box.Transform.RelativePadding = new Vector4(0.05f);
        this.Box.Load();

        var font = new ScalableFont(Styles.Fonts.Paths.Main, 18)
        {
            AutoResize = true,
            Spacing = 8,
        };

        this.text = new WrappedText(font, Color.White)
        {
            Parent = this.Box,
            Case = TextCase.Upper,
            TextAlignment = Alignment.Center,
            LineSpacing = 4,
            AdjustTransformSizeToText = AdjustSizeOption.OnlyHeight,
            Transform =
            {
                Alignment = Alignment.Center,
            },
        };

        this.Background = new SolidColor(GameClientCore.ThemeColor)
        {
            Opacity = 0.3f,
        };

        this.Background.Load();

        if (cancellationTokenSource is not null)
        {
            _ = cancellationTokenSource.Token.Register(() =>
            {
                ScreenController.HideOverlay(this);
            });

            this.CreateCloseButton(cancellationTokenSource);
        }
    }

    /// <summary>
    /// Updates the message box.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.UpdateText(gameTime);

        base.Update(gameTime);
    }

    private void UpdateText(GameTime gameTime)
    {
        string locale = Localization.Get("Other.Connecting")!;
        string[] words = locale.Split(' ');

        var sb = words.Length < 2
            ? new StringBuilder(locale)
            : new StringBuilder()
                .AppendJoin(' ', words[..^1])
                .Append('\n')
                .Append(words[^1]);

        var dotCount = (int)((gameTime.TotalGameTime.TotalSeconds % 2f) * 2);
        _ = sb.AppendJoin(null, Enumerable.Repeat('.', dotCount));

        this.text.Value = sb.ToString();
    }

    private void CreateCloseButton(CancellationTokenSource cancellationTokenSource)
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
        button.Clicked += (s, e) =>
        {
            cancellationTokenSource.Cancel();
        };

        icon.Load();
    }
}
