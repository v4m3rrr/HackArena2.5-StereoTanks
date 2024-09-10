using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a grid state payload.
/// </summary>
public class GameStatePayload : IPacketPayload
{
    public GameStatePayload(List<Player> players, Grid.StatePayload gridState)
    {
        this.Players = players;
        this.GridState = gridState;
    }

    /// <inheritdoc/>
    public PacketType Type => PacketType.GameState;

    /// <summary>
    /// Gets the players.
    /// </summary>
    public List<Player> Players { get; init; }

    /// <summary>
    /// Gets the grid state.
    /// </summary>
    [JsonProperty("grid")]
    public Grid.StatePayload GridState { get; init; }

    /// <summary>
    /// Gets the converters to use during serialization.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <returns>The list of converters to use during serialization.</returns>
    public static List<JsonConverter> GetConverters(SerializationContext context)
    {
        return [new TankJsonConverter(context),
            new TurretJsonConverter(context),
            new GridStateJsonConverter(context),
            new BulletJsonConverter(context),
            new WallJsonConverter(context),
            new ZoneJsonConverter(context),
            new PlayerJsonConverter(context)];
    }

    /// <summary>
    /// Represents a grid state payload for a specific player.
    /// </summary>
    public class ForPlayer : GameStatePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForPlayer"/> class.
        /// </summary>
        /// <param name="player">The player the payload is for.</param>
        /// <param name="players">The list of all players.</param>
        /// <param name="gridState">The grid state of the game.</param>
        public ForPlayer(Player player, List<Player> players, Grid.StatePayload gridState)
            : base(players, gridState)
        {
            this.PlayerId = player.Id;
            this.RegenProgress = player.Tank.RegenProgress;
            this.VisibilityGrid = player.VisibilityGrid;
        }

        [JsonConstructor]
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Used by Newtonsoft.Json.")]
        private ForPlayer(
            List<Player> players,
            Grid.StatePayload gridState,
            string playerId,
            float? regenProgress,
            int[,] visibilityGrid)
            : base(players, gridState)
        {
            this.PlayerId = playerId;
            this.RegenProgress = regenProgress;

            // To avoid compiler warning.
            // This is set in the VisibilityGridAsInt property.
            this.VisibilityGrid = default!;

            this.VisibilityGridAsInt = visibilityGrid;
        }

        /// <summary>
        /// Gets the id of the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the regeneration progress of the player's tank.
        /// </summary>
        public float? RegenProgress { get; }

        /// <summary>
        /// Gets the visibility grid of the player.
        /// </summary>
        [JsonIgnore]
        public bool[,] VisibilityGrid { get; private init; }

        /// <summary>
        /// Gets the visibility grid of the player as an integer array.
        /// </summary>
        [JsonProperty("visibilityGrid")]
        public int[,] VisibilityGridAsInt
        {
            get
            {
                int rows = this.VisibilityGrid.GetLength(0);
                int cols = this.VisibilityGrid.GetLength(1);
                int[,] visibilityGridAsInt = new int[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        visibilityGridAsInt[i, j] = this.VisibilityGrid[i, j] ? 1 : 0;
                    }
                }

                return visibilityGridAsInt;
            }

            init
            {
                int rows = value.GetLength(0);
                int cols = value.GetLength(1);
                bool[,] visibilityGrid = new bool[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        visibilityGrid[i, j] = value[i, j] == 1;
                    }
                }

                this.VisibilityGrid = visibilityGrid;
            }
        }
    }
}
