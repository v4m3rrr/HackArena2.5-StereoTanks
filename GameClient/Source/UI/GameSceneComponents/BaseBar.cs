using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

/// <summary>
/// Represents a base class for bars in the game scene.
/// </summary>
internal abstract class BaseBar : Component
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseBar"/> class.
    /// </summary>
    protected BaseBar()
    {
        this.Background = new RoundedSolidColor(GameClientCore.ThemeColor, 24)
        {
            Parent = this,
            Opacity = 0.35f,
            AutoAdjustRadius = true,
            Transform =
            {
                IgnoreParentPadding = true,
            },
        };

        this.Container = new Container()
        {
            Parent = this.Background,
        };
    }

    /// <summary>
    /// Gets the container for the bar's components.
    /// </summary>
    protected RoundedSolidColor Background { get; }

    /// <summary>
    /// Gets the container for the bar's components.
    /// </summary>
    protected Container Container { get; }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        this.Background.Draw(gameTime);
    }
}
