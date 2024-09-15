using GameClient.GameSceneComponents.PlayerBarComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

/// <summary>
/// Represents a player stats bar.
/// </summary>
internal class PlayerStatsBar : PlayerBar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerStatsBar"/> class.
    /// </summary>
    /// <param name="player">The player the bar represents.</param>
    public PlayerStatsBar(Player player)
        : base(player)
    {
        // Bullet count
        _ = new BulletCount(player)
        {
            Parent = this.Background,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.35f),
                Alignment = Alignment.TopLeft,
                RelativeOffset = new Vector2(0.05f, 0.1f),
            },
        };

        // Score
        _ = new Score(player)
        {
            Parent = this.Background,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.TopRight,
            },
        };

        // Health bar
        _ = new HealthBar(player)
        {
            Parent = this.Background,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Bottom,
            },
        };
    }
}
