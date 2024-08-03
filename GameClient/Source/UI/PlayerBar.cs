using GameClient.PlayerBarComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents a player bar.
/// </summary>
internal class PlayerBar : Component
{
    private const int Rows = 3;
    private readonly Frame baseFrame;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerBar"/> class.
    /// </summary>
    /// <param name="player">The player the bar represents.</param>
    public PlayerBar(Player player)
    {
        this.Player = player;

        this.baseFrame = new Frame(Color.White * 0.75f, 1)
        {
            Parent = this,
            Transform = { RelativePadding = new Vector4(0.01f, 0.005f, 0.01f, 0.005f) },
        };

        // Background
        _ = new SolidColor(Color.Black * 0.15f)
        {
            Parent = this.baseFrame.InnerContainer,
            Transform = { IgnoreParentPadding = true },
        };

        // Nickname
        _ = new Nickname(player)
        {
            Parent = this.baseFrame.InnerContainer,
            Transform =
            {
                RelativeSize = new Vector2(1, 1f / Rows),
                Alignment = Alignment.TopLeft,
            },
        };

        // Ping
        _ = new Ping(player)
        {
            Parent = this.baseFrame.InnerContainer,
            Transform =
            {
                RelativeSize = new Vector2(1f, 1f / Rows),
                Alignment = Alignment.TopRight,
            },
        };

        // Score
        _ = new Score(player)
        {
            Parent = this.baseFrame.InnerContainer,
            Transform =
            {
                RelativeSize = new Vector2(1f, 1f / Rows),
                Alignment = Alignment.Right,
            },
        };

        // Bullet count
        _ = new BulletCount(player)
        {
            Parent = this.baseFrame.InnerContainer,
            Transform =
            {
                RelativeSize = new Vector2(1f, 1f / Rows),
                Alignment = Alignment.Left,
            },
        };

        // Health bar
        _ = new HealthBar(player)
        {
            Parent = this.baseFrame.InnerContainer,
            Transform =
            {
                RelativeSize = new Vector2(1f, 1f / Rows),
                Alignment = Alignment.BottomLeft,
                IgnoreParentPadding = true,
            },
        };
    }

    /// <summary>
    /// Gets the player the bar represents.
    /// </summary>
    public Player Player { get; private set; }
}
