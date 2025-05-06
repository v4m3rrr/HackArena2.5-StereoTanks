using System.Diagnostics.CodeAnalysis;
using GameLogic.Networking.GameState;
using Newtonsoft.Json;

namespace GameLogic.Networking;

/// <summary>
/// Represents a grid state payload.
/// </summary>
public class GameStatePayload : IPacketPayload
{
#if STEREO

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStatePayload"/> class.
    /// </summary>
    /// <param name="tick">The current tick.</param>
    /// <param name="teams">The list of teams.</param>
    /// <param name="grid">The grid state.</param>
    public GameStatePayload(int tick, List<Team> teams, Grid grid)
        : this(tick, teams, grid.ToMapPayload(null))
    {
    }

    [JsonConstructor]
    private GameStatePayload(int tick, List<Team> teams, Grid.MapPayload map)
    {
        this.Tick = tick;
        this.Teams = teams;
        this.Map = map;
    }

#else

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStatePayload"/> class.
    /// </summary>
    /// <param name="tick">The current tick.</param>
    /// <param name="players">The list of players.</param>
    /// <param name="grid">The grid state.</param>
    public GameStatePayload(int tick, List<Player> players, Grid grid)
        : this(tick, players, grid.ToMapPayload(null))
    {
    }

    [JsonConstructor]
    private GameStatePayload(int tick, List<Player> players, Grid.MapPayload map)
    {
        this.Tick = tick;
        this.Players = players;
        this.Map = map;
    }

#endif

    /// <inheritdoc/>
    public PacketType Type => PacketType.GameState;

    /// <summary>
    /// Gets the number of ticks that
    /// have passed in the game.
    /// </summary>
    public int Tick { get; }

#if STEREO

    /// <summary>
    /// Gets the teams.
    /// </summary>
    public List<Team> Teams { get; }

    /// <summary>
    /// Gets the players.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Player> Players => this.Teams.SelectMany(t => t.Players);

#else

    /// <summary>
    /// Gets the players.
    /// </summary>
    public List<Player> Players { get; }

#endif

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
        return [
#if STEREO
            new TankJsonConverter(context),
            new HeavyTankJsonConverter(context),
            new LightTankJsonConverter(context),
            new HeavyTurretJsonConverter(context),
            new LightTurretJsonConverter(context),
            new TeamJsonConverter(context),
#else
            new TurretJsonConverter(context),
#endif
            new GridTilesJsonConverter(context),
            new GridVisibilityJsonConverter(),
            new MapJsonConverter(context),
            new BulletJsonConverter(context),
            new LaserJsonConverter(context),
            new MineJsonConverter(context),
#if !STEREO
            new ItemJsonConverter(context),
#endif
            new WallJsonConverter(context),
            new ZoneJsonConverter(context),
            new PlayerJsonConverter(context)];
    }

    /// <summary>
    /// Represents a grid state payload for a specific player.
    /// </summary>
    public class ForPlayer : GameStatePayload
    {
#if STEREO

        /// <summary>
        /// Initializes a new instance of the <see cref="ForPlayer"/> class.
        /// </summary>
        /// <param name="id">The packet id.</param>
        /// <param name="tick">The current tick.</param>
        /// <param name="player">The player the payload is for.</param>
        /// <param name="teams">The list of teams.</param>
        /// <param name="grid">The grid state.</param>
        public ForPlayer(string id, int tick, Player player, List<Team> teams, Grid grid)
            : this(id, tick, teams, grid.ToMapPayload(player))
        {
        }

        [JsonConstructor]
        private ForPlayer(string id, int tick, List<Team> teams, Grid.MapPayload map)
            : base(tick, teams, map)
        {
            this.Id = id;
        }

#else

        /// <summary>
        /// Initializes a new instance of the <see cref="ForPlayer"/> class.
        /// </summary>
        /// <param name="id">The packet id.</param>
        /// <param name="tick">The current tick.</param>
        /// <param name="player">The player the payload is for.</param>
        /// <param name="players">The list of players.</param>
        /// <param name="grid">The grid state.</param>
        public ForPlayer(string id, int tick, Player player, List<Player> players, Grid grid)
            : this(id, tick, players, grid.ToMapPayload(player))
        {
        }

        [JsonConstructor]
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Used by Newtonsoft.Json.")]
        private ForPlayer(string id, int tick, List<Player> players, Grid.MapPayload map)
            : base(tick, players, map)
        {
            this.Id = id;
        }

#endif

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
