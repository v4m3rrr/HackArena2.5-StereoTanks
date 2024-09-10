using GameClient.PlayerBarComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents a player identity bar.
/// </summary>
internal class PlayerIdentityBar : PlayerBar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerIdentityBar"/> class.
    /// </summary>
    /// <param name="player">The player the bar represents.</param>
    public PlayerIdentityBar(Player player)
        : base(player)
    {
        // Nickname
        _ = new Nickname(player)
        {
            Parent = this.Background,
            Transform =
            {
                RelativeSize = new Vector2(0.8f, 0.5f),
                Alignment = Alignment.TopLeft,
            },
        };

        // Ping
        _ = new Ping(player)
        {
            Parent = this.Background,
            Transform =
            {
                RelativeSize = new Vector2(0.3f, 0.5f),
                Alignment = Alignment.BottomLeft,
            },
        };

        // Tank sprite
        _ = new TankSprite(player)
        {
            Parent = this.Background,
            Transform =
            {
                RelativeSize = new Vector2(0.3f, 1f),
                Ratio = new Ratio(1, 1),
                Alignment = Alignment.Right,
            },
        };
    }
}
