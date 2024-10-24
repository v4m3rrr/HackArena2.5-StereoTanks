using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

/// <summary>
/// Represents a player's score displayed on a player bar.
/// </summary>
internal class Score : PlayerBarComponent
{
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 12)
    {
        AutoResize = true,
        Spacing = 5,
    };

    private readonly Text text;

    /// <summary>
    /// Initializes a new instance of the <see cref="Score"/> class.
    /// </summary>
    /// <param name="player">The player whose score will be displayed.</param>
    public Score(Player player)
        : base(player)
    {
        this.text = new Text(Font, Color.White)
        {
            Parent = this,
            Value = player.Score.ToString(),
            TextAlignment = Alignment.Right,
        };
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        this.text.Value = this.Player.Score.ToString();

        base.Update(gameTime);
    }
}
