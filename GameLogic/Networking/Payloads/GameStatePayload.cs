using GameLogic.Networking.GameState;
using GameLogic.Networking.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogic.Networking;

/// <summary>
/// Represents a grid state payload.
/// </summary>
internal class GameStatePayload : IPacketPayload
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
    private GameStatePayload(int tick, List<Team> teams, MapPayload map)
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
    private GameStatePayload(int tick, List<Player> players, MapPayload map)
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
    /// Gets or sets the map state.
    /// </summary>
    [JsonProperty]
    internal MapPayload Map { get; set; }

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
            new TankJsonConverter(context),
#if STEREO
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
            new ZoneJsonConverter(),
#if STEREO
            new ZoneSharesJsonConverter(),
#endif
            new PlayerJsonConverter(context)];
    }

    /// <summary>
    /// Represents a grid state payload for a specific player.
    /// </summary>
    public class ForPlayer : GameStatePayload
    {
#if STEREO

        private bool[,]? visibilityGrid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForPlayer"/> class.
        /// </summary>
        /// <param name="id">The packet id.</param>
        /// <param name="tick">The current tick.</param>
        /// <param name="player">The player the payload is for.</param>
        /// <param name="teams">The list of teams.</param>
        /// <param name="grid">The grid state.</param>
        public ForPlayer(string id, int tick, Player player, List<Team> teams, Grid grid)
            : this(id, tick, teams, grid.ToMapPayload(player), player.Id)
        {
        }

        [JsonConstructor]
        private ForPlayer(string id, int tick, List<Team> teams, MapPayload map, string playerId)
            : base(tick, teams, map)
        {
            this.Id = id;
            this.PlayerId = playerId;
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
            : this(id, tick, players, grid.ToMapPayload(player), player.Id)
        {
        }

        [JsonConstructor]
        private ForPlayer(string id, int tick, List<Player> players, MapPayload map, string playerId)
            : base(tick, players, map)
        {
            this.Id = id;
            this.PlayerId = playerId;
        }

#endif

        /// <summary>
        /// Gets the packet id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the player id.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the player visibility grid.
        /// </summary>
        [JsonIgnore]
#if !STEREO
        /* Backwards compatibility */
        public bool[,] VisibilityGrid => this.Map.Visibility!.Grid;
#else
        public bool[,] VisibilityGrid => this.visibilityGrid ??= this.GetVisibilityGrid();
#endif

#if STEREO

        /// <summary>
        /// Parses the raw JSON game state payload and extracts a mapping of player IDs to team names.
        /// </summary>
        /// <param name="rawGameStatePayload">The raw game state payload JSON object.</param>
        /// <returns>
        /// A dictionary where keys are player IDs and values are their corresponding team names.
        /// </returns>
        public static Dictionary<string, string> GetPlayerTeamMap(JObject rawGameStatePayload)
        {
            return rawGameStatePayload["teams"]!
                .SelectMany(team => team["players"]!.Select(player => new
                {
                    PlayerId = player["id"]!.Value<string>()!,
                    TeamName = team["name"]!.Value<string>()!,
                }))
                .ToDictionary(p => p.PlayerId, p => p.TeamName);
        }

        private bool[,] GetVisibilityGrid()
        {
            bool[,] grid = null!;

            foreach (var team in this.Teams)
            {
                if (team.CombinedVisibilityGrid is null)
                {
                    continue;
                }

                if (grid is null)
                {
                    grid = team.CombinedVisibilityGrid;
                    continue;
                }

                for (int i = 0; i < grid.GetLength(0); i++)
                {
                    for (int j = 0; j < grid.GetLength(1); j++)
                    {
                        grid[i, j] |= team.CombinedVisibilityGrid[i, j];
                    }
                }
            }

            return grid;
        }

#endif
    }
}
