namespace GameLogic.Networking;

/// <summary>
/// Represents a game serialization context.
/// </summary>
public abstract class GameSerializationContext(EnumSerializationFormat enumSerialization)
    : SerializationContext(enumSerialization)
{
    /// <summary>
    /// Gets a value indicating whether the context is a spectator.
    /// </summary>
    public bool IsSpectator => this is Spectator;

    /// <summary>
    /// Gets a value indicating whether the context is a player.
    /// </summary>
    public bool IsPlayer => this is Player;

    /// <summary>
    /// Determines whether the context is a player with the specified id.
    /// </summary>
    /// <param name="id">The id to check.</param>
    /// <returns>
    /// <see langword="true"/> if the context is a player with the specified id;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsPlayerWithId(string id)
    {
        return this is Player player && player.Id == id;
    }

#if STEREO

    /// <summary>
    /// Determines whether the context is a player of the same team.
    /// </summary>
    /// <param name="teammateId">The id of the teammate to check against the current player.</param>
    /// <returns>
    /// <see langword="true"/> if the context is a player of the same team;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsTeammate(string teammateId)
    {
        return this is Player player
            && player.Id != teammateId
            && player.PlayerTeamMap.TryGetValue(teammateId, out var teamName)
            && teamName == player.PlayerTeamMap[player.Id];
    }

#endif

    /// <summary>
    /// Represents a player serialization context.
    /// </summary>
    public class Player : GameSerializationContext
    {
#if CLIENT

        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="id">The id of the player.</param>
        public Player(string id)
            : base(EnumSerializationFormat.Int)
        {
            this.Id = id;
        }

#endif

#if SERVER
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="player">The player to create the context for.</param>
        /// <param name="enumSerialization">The enum serialization format.</param>
        public Player(GameLogic.Player player, EnumSerializationFormat enumSerialization)
            : base(enumSerialization)
        {
            this.Id = player.Id;

#if STEREO
            this.IsUsingRadar = player.Tank.GetAbility<RadarAbility>()?.IsActive!;
            this.VisibilityGrid = player.Team.CombinedVisibilityGrid;
            this.PlayerTeamMap = player.Team.Players.ToDictionary(p => p.Id, p => player.Team.Name);
#else
            this.IsUsingRadar = player.Tank.GetAbility<RadarAbility>()?.IsActive ?? false;
            this.VisibilityGrid = player.Tank.VisibilityGrid;
#endif
        }

#endif

            /// <summary>
            /// Gets the id of the player.
            /// </summary>
        public string Id { get; }

#if STEREO

        /// <summary>
        /// Gets the team name of the player.
        /// </summary>
        public
#if CLIENT
        required
#endif
        IReadOnlyDictionary<string, string> PlayerTeamMap
        {
            get;
#if CLIENT
            init;
#endif
        }

#endif

            /// <summary>
            /// Gets a value indicating whether the player is using radar.
            /// </summary>
#if STEREO
        public bool? IsUsingRadar { get; }
#else
        public bool IsUsingRadar { get; }
#endif

        /// <summary>
        /// Gets the visibility grid of the player.
        /// </summary>
        public bool[,]? VisibilityGrid { get; }
    }

    /// <summary>
    /// Represents a spectator serialization context.
    /// </summary>
    public class Spectator() : GameSerializationContext(EnumSerializationFormat.Int)
    {
    }
}
