using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

/// <summary>
/// Represents a player bar.
/// </summary>
internal abstract class PlayerBar : Component
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerBar"/> class.
    /// </summary>
    /// <param name="player">The player the bar represents.</param>
    protected PlayerBar(Player player)
    {
        this.Player = player;

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
    /// Gets the player the bar represents.
    /// </summary>
    public Player Player { get; }

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
