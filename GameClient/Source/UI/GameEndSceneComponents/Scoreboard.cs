using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI.GameEndSceneComponents;

/// <summary>
/// Represents a player slot panel.
/// </summary>
internal class Scoreboard : Component
{
    private readonly AlignedListBox listBox;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scoreboard"/> class.
    /// </summary>
    public Scoreboard()
    {
        this.listBox = new AlignedListBox
        {
            Parent = this,
            Orientation = Orientation.Vertical,
            Spacing = 10,
            ElementsAlignment = Alignment.Center,
        };
    }

    /// <summary>
    /// Sets the players to display on the scoreboard.
    /// </summary>
    /// <param name="players">
    /// The sorted players to display on the scoreboard.
    /// </param>
    public void SetPlayers(Player[] players)
    {
        this.listBox.Clear();

        for (int i = 0; i < players.Length; i++)
        {
#if HACKATON
            var isQualified = i < 2;
            var slot = new PlayerSlotPanel(players[i], isQualified)
#else
            var slot = new PlayerSlotPanel(players[i])
#endif
            {
                Parent = this.listBox.ContentContainer,
                Transform =
                {
                    RelativeSize = new Vector2(1.0f, 0.22f),
                },
            };
        }
    }
}
