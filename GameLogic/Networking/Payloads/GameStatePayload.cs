using System.Diagnostics;

namespace GameLogic.Networking;

/// <summary>
/// Represents a grid state payload.
/// </summary>
public class GameStatePayload : IPacketPayload
{
    private Grid.StatePayload gridState = default!;

    /// <inheritdoc/>
    public PacketType Type => PacketType.GameState;

    /// <summary>
    /// Gets the players.
    /// </summary>
    public List<Player> Players { get; init; } = [];

    /// <summary>
    /// Gets the grid state.
    /// </summary>
    public Grid.StatePayload GridState
    {
        get => this.gridState;
        init
        {
            foreach (Tank tank in value.Tanks)
            {
                var owner = this.Players.Find(p => p.Id == tank.OwnerId);
                if (owner is null)
                {
                    Debug.Fail("Owner not found for tank.");
                    continue;
                }

                tank.Owner = owner;
                tank.Owner.Tank = tank;
            }

            foreach (Bullet bullet in value.Bullets)
            {
                var owner = this.Players.Find(p => p.Id == bullet.ShooterId);
                if (owner is null)
                {
                    Debug.Fail("Owner not found for bullet.");
                    continue;
                }

                bullet.Shooter = owner;
            }

            this.gridState = value;
        }
    }
}
