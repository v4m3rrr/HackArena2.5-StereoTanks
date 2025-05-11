using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

#if !STEREO

/// <summary>
/// Represents a player's nickname displayed on a player bar.
/// </summary>
internal class Nickname : PlayerBarComponent
{
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 13)
    {
        AutoResize = true,
        Spacing = 7,
    };

    private readonly Text text;

    /// <summary>
    /// Initializes a new instance of the <see cref="Nickname"/> class.
    /// </summary>
    /// <param name="player">The player whose nickname will be displayed.</param>
    public Nickname(Player player)
        : base(player)
    {
        this.text = new Text(Font, Color.White)
        {
            Parent = this,
            Value = player.Nickname,
            TextAlignment = Alignment.TopLeft,
            TextShrink = TextShrinkMode.Width,
        };
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        base.Update(gameTime);

        this.text.Color = this.Player.IsTankDead ? Color.DarkGray : Color.White;
    }
}

#endif
