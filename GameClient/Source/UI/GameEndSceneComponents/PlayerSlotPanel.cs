using GameClient.UI.SceneComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI.GameEndSceneComponents;

/// <summary>
/// Represents a player slot panel.
/// </summary>
/// <remarks>
/// The player slot panel displays the player's nickname,
/// score and tank icon on the game end screen.
/// </remarks>
internal class PlayerSlotPanel : Component
{
    private readonly RoundedSolidColor background;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerSlotPanel"/> class.
    /// </summary>
    /// <param name="player">The player to display.</param>
    public PlayerSlotPanel(Player player)
    {
        var color = new Color(player.Color);

        this.background = new RoundedSolidColor(GameClientCore.ThemeColor, 20)
        {
            AutoAdjustRadius = true,
            Parent = this,
            Opacity = 0.45f,
        };

        var iconBackground = new RoundedSolidColor(Color.White, 15)
        {
            Parent = this,
            Opacity = 0.3f,
            Transform =
            {
                Alignment = Alignment.Left,
                Ratio = new Ratio(1, 1),
            },
        };

        var tankSpriteIcon = new TankSpriteIcon()
        {
            Parent = iconBackground,
            Transform =
            {
                RelativeSize = new Vector2(0.55f),
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };

        tankSpriteIcon.SetColor(color);

        var font = new ScalableFont(Styles.Fonts.Paths.Main, 19)
        {
            AutoResize = true,
            Spacing = 8,
        };

        var nickContainer = new Container()
        {
            Parent = this,
            Transform =
            {
                RelativeSize = new Vector2(0.82f, 1f),
                Alignment = Alignment.Right,
            },
        };

        // Nickname
        _ = new Text(font, Color.White)
        {
            Parent = nickContainer,
            Value = player.Nickname,
            Case = TextCase.Upper,
            TextAlignment = Alignment.Left,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Transform =
            {
                RelativeSize = new Vector2(0.9f, 0.6f),
                Alignment = Alignment.Left,
            },
        };

        // Score
        _ = new Text(font, color)
        {
            Parent = this.background,
            Value = player.Score.ToString(),
            TextAlignment = Alignment.Right,
            Transform =
            {
                RelativeSize = new Vector2(0.9f, 0.6f),
                Alignment = Alignment.Right,
                RelativeOffset = new Vector2(-0.05f, 0.0f),
            },
        };
    }
}
