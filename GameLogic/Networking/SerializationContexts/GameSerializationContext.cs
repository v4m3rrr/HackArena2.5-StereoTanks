namespace GameLogic.Networking;

/// <summary>
/// Represents a game serialization context.
/// </summary>
public abstract class GameSerializationContext
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="id">The id of the player.</param>
        /// <remarks>
        /// This constructor should be used on the client side.
        /// </remarks>
        public Player(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="player">The player to create the context for.</param>
        /// <remarks>
        /// This constructor should be used on the server side.
        /// </remarks>
        public Player(GameLogic.Player player)
        {
            this.Id = player.Id;
            this.VisibilityGrid = player.VisibilityGrid;
            this.IsUsingRadar = player.IsUsingRadar;
        }

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
        public bool IsUsingRadar { get; }
    }

    /// <summary>
    /// Represents a spectator serialization context.
    /// </summary>
    public class Spectator : GameSerializationContext
    {
    }
}
