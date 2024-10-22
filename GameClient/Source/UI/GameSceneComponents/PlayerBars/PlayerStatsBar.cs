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
        this.Background.Transform.RelativePadding
            = new Vector4(0.06f, 0.25f, 0.06f, 0.14f);

        var topRow = new FlexListBox
        {
            Parent = this.Container,
            Orientation = MonoRivUI.Orientation.Horizontal,
            Spacing = 5,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Top,
            },
        };

        var bulletCount = new BulletCount(player)
        {
            Parent = topRow.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.19f),
            },
        };

        var secondaryItems = new SecondaryItems(player)
        {
            Parent = topRow.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.06f),
            },
        };

        var score = new Score(player)
        {
            Parent = topRow.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.1f),
            },
        };

        topRow.SetResizeFactor(bulletCount, 2.6f);
        topRow.SetResizeFactor(secondaryItems, 7);
        topRow.SetResizeFactor(score, 4);

        // Health bar
        _ = new HealthBar(player)
        {
            Parent = this.Container,
            Transform =
            {
                RelativeSize = new Vector2(1f, 0.5f),
                Alignment = Alignment.Bottom,
            },
        };
    }
}
