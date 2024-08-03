namespace GameLogic.Networking;

/// <summary>
/// Represents a serialization context.
/// </summary>
public abstract class SerializationContext
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
    /// <param name="id">The id of the player.</param>
    public class Player(string id) : SerializationContext
    {
        /// <summary>
        /// Gets the id of the player.
        /// </summary>
        public string Id { get; } = id;
    }

    /// <summary>
    /// Represents a spectator serialization context.
    /// </summary>
    public class Spectator : SerializationContext
    {
    }
}
