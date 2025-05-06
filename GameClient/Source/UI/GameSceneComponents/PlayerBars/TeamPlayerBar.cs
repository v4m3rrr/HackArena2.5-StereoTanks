using GameClient.GameSceneComponents.PlayerBarComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents;

#if STEREO

/// <summary>
/// Represents a player bar for a team.
/// </summary>
internal class TeamPlayerBar : PlayerBar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeamPlayerBar"/> class.
    /// </summary>
    /// <param name="player">The team player the bar represents.</param>
    public TeamPlayerBar(Player player)
        : base(player)
    {
        this.Background.Transform.RelativePadding
            = new Vector4(0.04f, 0.2f, 0.04f, 0.14f);

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

        var abilities = new Abilities(player)
        {
            Parent = topRow.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(0.001f),
            },
        };

        var spaceContainer = new Container()
        {
            Parent = topRow.ContentContainer,
        };

        var tankSprite = new TankSprite(player)
        {
            Parent = topRow.ContentContainer,
            Transform =
            {
                RelativePadding = new Vector4(-0.21f),
            },
        };

        topRow.SetResizeFactor(bulletCount, 2.6f);
        topRow.SetResizeFactor(abilities, 2.6f);
        topRow.SetResizeFactor(spaceContainer, 3.2f);
        topRow.SetResizeFactor(tankSprite, 2.6f);

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

#endif
