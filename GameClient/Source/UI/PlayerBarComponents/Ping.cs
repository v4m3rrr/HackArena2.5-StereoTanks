using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.PlayerBarComponents;

/// <summary>
/// Represents a player's ping displayed on a player bar.
/// </summary>
internal class Ping : PlayerBarComponent
{
    private static readonly ScalableFont Font = new("Content\\Fonts\\Orbitron-SemiBold.ttf", 9);

    private readonly Text text;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ping"/> class.
    /// </summary>
    /// <param name="player">The player whose ping will be displayed.</param>
    public Ping(Player player)
        : base(player)
    {
        this.text = new Text(Font, new Color(this.Player.Color))
        {
            Parent = this,
            Value = $"-",
            TextAlignment = Alignment.Left,
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
        this.text.Value = ping < 1 ? "<1 MS" : $"{this.Player.Ping} MS";

        base.Update(gameTime);
    }
}
