using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents a player.
/// </summary>
public class Player : IEquatable<Player>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="id">The id of the player.</param>
    /// <param name="nickname">The nickname of the player.</param>
    /// <param name="color">The color of the player.</param>
    public Player(string id, string nickname, uint color)
    {
        this.Id = id;
        this.Nickname = nickname;
        this.Color = color;
    }

    /// <summary>
    /// Gets the id of the player.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Gets the nickname of the player.
    /// </summary>
    public string Nickname { get; private set; }

    /// <summary>
    /// Gets the score of the player.
    /// </summary>
    public int Score { get; internal set; } = 0;

    /// <summary>
    /// Gets the color of the player.
    /// </summary>
    public uint Color { get; internal set; }

    /// <summary>
    /// Gets or sets the ping of the player.
    /// </summary>
    public int Ping { get; set; }

    /// <summary>
    /// Gets the tank of the player.
    /// </summary>
    /// <remarks>
    /// This property has <see cref="JsonIgnoreAttribute"/> because it
    /// is set in the <see cref="Networking.GameStatePayload.GridState"/>
    /// init property when deserializing the game state,
    /// based on the <see cref="Tank.OwnerId"/> property.
    /// </remarks>
    [JsonIgnore]
    public Tank Tank { get; internal set; } = default!;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is equal to the current object;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Player);
    }

    /// <inheritdoc cref="Equals(object)"/>
    /// <remarks>
    /// The players are considered equal if they have the same id.
    /// </remarks>
    public bool Equals(Player? other)
    {
        return this.Id == other?.Id;
    }

    /// <summary>
    /// Gets the hash code of the player.
    /// </summary>
    /// <returns>The hash code of the player.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id);
    }

    /// <summary>
    /// Updates the player from another player.
    /// </summary>
    /// <param name="player">The player to update from.</param>
    public void UpdateFrom(Player player)
    {
        this.Nickname = player.Nickname;
        this.Color = player.Color;
        this.Score = player.Score;
        this.Ping = player.Ping;
        this.Tank = player.Tank;
    }
}
