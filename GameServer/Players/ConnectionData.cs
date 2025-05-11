using GameLogic;
using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the connection data.
/// </summary>
/// <param name="EnumSerialization">The enum serialization format.</param>
internal record class ConnectionData(EnumSerializationFormat EnumSerialization)
{
#if DEBUG

    /// <summary>
    /// Gets a value indicating whether
    /// the connection was made by quick join.
    /// </summary>
    public bool QuickJoin { get; init; }

#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionData"/> class.
    /// </summary>
    /// <param name="Type">The type of the player.</param>
    /// <param name="EnumSerialization">The enum serialization format.</param>
    public record class Player(PlayerType Type, EnumSerializationFormat EnumSerialization)
        : ConnectionData(EnumSerialization)
    {
#if STEREO

        /// <summary>
        /// Gets the team name of the player.
        /// </summary>
        public required string TeamName { get; init; }

        /// <summary>
        /// Gets the tank type of the player.
        /// </summary>
        public required TankType TankType { get; init; }

#else

        /// <summary>
        /// Gets the nickname of the player.
        /// </summary>
        public required string Nickname { get; init; }

#endif
    }
}
