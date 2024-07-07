using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents a localized text component.
/// </summary>
internal class LocalizedText : Text, ILocalizable
{
    private LocalizedString localizedString = LocalizedString.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedText"/> class.
    /// </summary>
    /// <param name="font">The font of the text.</param>
    /// <param name="color">The color of the displayed text.</param>
    public LocalizedText(ScalableFont font, Color color)
        : base(font, color)
    {
        ILocalizable.AddReference(this);
    }

    /// <summary>
    /// Gets or sets the value of the text.
    /// </summary>
    public new LocalizedString Value
    {
        get => this.localizedString;
        set
        {
            this.localizedString = value;
            base.Value = value.Value;
        }
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        base.Value = this.localizedString.Value;
    }
}
