using System.Linq;
using System.Text;
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
    public ConnectingMessageBox()
        : base(new((MonoTanks.ThemeColor * 0.66f).WithAlpha(255), 15))
    {
        this.CanBeClosedByClickOutside = false;

        this.Box.Transform.RelativeSize = new Vector2(0.30f, 0.15f);
        this.Box.Transform.MinSize = new Point(400, 200);
        this.Box.Transform.MaxSize = new Point(1000, 500);
        this.Box.Transform.Alignment = Alignment.Center;
        this.Box.Transform.RelativePadding = new Vector4(0.04f);

        var font = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 14)
        {
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

        this.Background = new SolidColor(MonoTanks.ThemeColor)
        {
            Opacity = 0.3f,
        };
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
        string locale = Localization.Get("Other.Connecting");
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
}
