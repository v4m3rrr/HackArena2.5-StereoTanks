using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.GameSceneComponents.PlayerBarComponents;

/// <summary>
/// Represents a player's ping displayed on a player bar.
/// </summary>
internal class Ping : PlayerBarComponent
{
    private static readonly ScalableFont Font = new(Styles.Fonts.Paths.Main, 8)
    {
        AutoResize = true,
        Spacing = 4,
    };

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
            TextAlignment = Alignment.BottomLeft,
            Spacing = 3,
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
