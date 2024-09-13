using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

/// <summary>
/// Represents a player's nickname displayed on a player bar.
/// </summary>
internal class Nickname : PlayerBarComponent
{
    private static readonly ScalableFont Font = new("Content\\Fonts\\Orbitron-SemiBold.ttf", 16);

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
            TextAlignment = Alignment.Left,
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

        this.text.Color = this.Player.Tank?.IsDead ?? true ? Color.DarkGray : Color.White;
    }
}
