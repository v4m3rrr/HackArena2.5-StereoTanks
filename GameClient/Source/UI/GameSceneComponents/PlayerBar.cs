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

        this.Background = new RoundedSolidColor(MonoTanks.ThemeColor, 15)
        {
            Parent = this,
            Opacity = 0.35f,
            Transform =
            {
                RelativePadding = new Vector4(0.05f),
            },
        };

        this.Background.Load();
    }

    /// <summary>
    /// Gets the player the bar represents.
    /// </summary>
    public Player Player { get; }

    /// <summary>
    /// Gets the background of the player bar.
    /// </summary>
    protected RoundedSolidColor Background { get; }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        this.Background.Draw(gameTime);
    }
}
