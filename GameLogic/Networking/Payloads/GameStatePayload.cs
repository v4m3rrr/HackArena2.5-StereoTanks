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
    /// <param name="tick">The current tick.</param>
    /// <param name="players">The list of players.</param>
    /// <param name="grid">The grid state.</param>
    public GameStatePayload(int tick, List<Player> players, Grid grid)
    {
        this.Tick = tick;
        this.Players = players;
        this.Map = grid.ToMapPayload(null);
    }

    [JsonConstructor]
    private GameStatePayload(int tick, List<Player> players, Grid.MapPayload map)
    {
        this.Tick = tick;
        this.Players = players;
        this.Map = map;
    }

    /// <inheritdoc/>
    public PacketType Type => PacketType.GameState;

    /// <summary>
    /// Gets the number of ticks that
    /// have passed in the game.
    /// </summary>
    public int Tick { get; }

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
        /// <param name="id">The packet id.</param>
        /// <param name="tick">The current tick.</param>
        /// <param name="player">The player the payload is for.</param>
        /// <param name="players">The list of players.</param>
        /// <param name="grid">The grid state.</param>
        public ForPlayer(string id, int tick, Player player, List<Player> players, Grid grid)
            : base(tick, players, grid.ToMapPayload(player))
        {
            this.Id = id;
        }

        [JsonConstructor]
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Used by Newtonsoft.Json.")]
        private ForPlayer(string id, int tick, List<Player> players, Grid.MapPayload map)
            : base(tick, players, map)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the packet id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the visibility grid of the player.
        /// </summary>
        [JsonIgnore]
        public bool[,] VisibilityGrid => this.Map.Visibility!.VisibilityGrid;
    }
}
