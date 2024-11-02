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
            Orientation = MonoRivUI.Orientation.Vertical,
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
            _ = new PlayerSlotPanel(players[i])
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
