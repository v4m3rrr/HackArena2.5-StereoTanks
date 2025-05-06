namespace GameLogic.Networking;

/// <summary>
/// Represents a game serialization context.
/// </summary>
public abstract class GameSerializationContext(EnumSerializationFormat enumSerialization)
    : SerializationContext(enumSerialization)
{
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
        /// <remarks>
        /// This constructor should be used on the client side.
        /// </remarks>
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
            this.VisibilityGrid = player.VisibilityGrid;
#if STEREO
            this.IsUsingRadar = (player.Tank as LightTank)?.IsUsingRadar;
#else
            this.IsUsingRadar = player.IsUsingRadar;
#endif
        }

#endif

        /// <summary>
        /// Gets the id of the player.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the visibility grid of the player.
        /// </summary>
        /// <remarks>
        /// On the client side, this property is always <see langword="null"/>.
        /// </remarks>
        public bool[,]? VisibilityGrid { get; }

        /// <summary>
        /// Gets a value indicating whether the player is using radar.
        /// </summary>
#if STEREO
        public bool? IsUsingRadar { get; }
#else
        public bool IsUsingRadar { get; }
#endif
    }

    /// <summary>
    /// Represents a spectator serialization context.
    /// </summary>
    public class Spectator() : GameSerializationContext(EnumSerializationFormat.Int)
    {
    }
}
