using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.PlayerBarComponents;

/// <summary>
/// Represents a player's ping displayed on a player bar.
/// </summary>
internal class Ping : PlayerBarComponent
{
    private static readonly ScalableFont Font = new("Content\\Fonts\\Tiny5-Regular.ttf", 13);

    private readonly Text text;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ping"/> class.
    /// </summary>
    /// <param name="player">The player whose ping will be displayed.</param>
    public Ping(Player player)
        : base(player)
    {
        this.text = new Text(Font, Color.Gray)
        {
            Parent = this,
            Value = $"-",
            TextAlignment = Alignment.Right,
            TextShrink = TextShrinkMode.Height,
        };
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!this.IsEnabled)
        {
            return;
        }

        var ping = this.Player.Ping;
        this.text.Value = ping < 1 ? "<1ms" : $"{this.Player.Ping}ms";

        base.Update(gameTime);
    }
}
