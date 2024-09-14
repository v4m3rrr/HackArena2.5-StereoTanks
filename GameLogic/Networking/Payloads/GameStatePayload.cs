using System.Diagnostics.CodeAnalysis;
using GameLogic.Networking.GameState;
using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a grid state payload.
/// </summary>
public class GameStatePayload : IPacketPayload
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameStatePayload"/> class.
    /// </summary>
    /// <param name="time">The time of the game.</param>
    /// <param name="players">The list of players.</param>
    /// <param name="grid">The grid state.</param>
    public GameStatePayload(float time, List<Player> players, Grid grid)
    {
        this.Time = time;
        this.Players = players;
        this.Map = grid.ToMapPayload(null);
    }

    [JsonConstructor]
    private GameStatePayload(float time, List<Player> players, Grid.MapPayload map)
    {
        this.Time = time;
        this.Players = players;
        this.Map = map;
    }

    /// <inheritdoc/>
    public PacketType Type => PacketType.GameState;

    /// <summary>
    /// Gets the time of the game.
    /// </summary>
    public float Time { get; }

    /// <summary>
    /// Gets the players.
    /// </summary>
    public List<Player> Players { get; }

    /// <summary>
    /// Gets the map state.
    /// </summary>
    [JsonProperty]
    internal Grid.MapPayload Map { get; }

    /// <summary>
    /// Gets the converters to use during
    /// serialization and deserialization.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <returns>
    /// The list of converters to use during
    /// serialization and deserialization.
    /// </returns>
    public static List<JsonConverter> GetConverters(GameSerializationContext context)
    {
        return [new TankJsonConverter(context),
            new TurretJsonConverter(context),
            new GridTilesJsonConverter(context),
            new GridVisibilityJsonConverter(context),
            new MapJsonConverter(context),
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
        /// <param name="time">The time of the game.</param>
        /// <param name="player">The player the payload is for.</param>
        /// <param name="players">The list of players.</param>
        /// <param name="grid">The grid state.</param>
        public ForPlayer(float time, Player player, List<Player> players, Grid grid)
            : base(time, players, grid.ToMapPayload(player))
        {
            this.PlayerId = player.Id;
        }

        [JsonConstructor]
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Used by Newtonsoft.Json.")]
        private ForPlayer(float time, List<Player> players, Grid.MapPayload map, string playerId)
            : base(time, players, map)
        {
            this.PlayerId = playerId;
        }

        /// <summary>
        /// Gets the id of the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the visibility grid of the player.
        /// </summary>
        [JsonIgnore]
        public bool[,] VisibilityGrid => this.Map.Visibility!.VisibilityGrid;
    }
}
