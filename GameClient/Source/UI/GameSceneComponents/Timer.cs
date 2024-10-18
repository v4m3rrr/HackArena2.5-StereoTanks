using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

/// <summary>
/// Represents a timer displayed on the game scene.
/// </summary>
internal class Timer : Component
{
    private readonly Text text;

    /// <summary>
    /// Initializes a new instance of the <see cref="Timer"/> class.
    /// </summary>
    public Timer()
    {
        // Timer icon
        _ = new ScalableTexture2D("Images/Icons/timer.svg")
        {
            Parent = this,
            Transform =
            {
                Alignment = Alignment.Left,
                RelativeSize = new Vector2(0.94f),
            },
        };

        var timeFont = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 16);
        this.text = new Text(timeFont, Color.White)
        {
            Parent = this,
            Value = "00:00",
            Spacing = 5,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.Left,
                RelativeOffset = new Vector2(0.4f, 0.0f),
                RelativeSize = new Vector2(0.6f, 0.94f),
            },
        };
    }

    /// <summary>
    /// Gets or sets the time in milliseconds.
    /// </summary>
    public float Time { get; set; }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        var minutes = (int)(this.Time / 60000);
        var seconds = (int)(this.Time / 1000) % 60;
        this.text.Value = $"{minutes:D2}:{seconds:D2}";

        base.Update(gameTime);
    }
}
