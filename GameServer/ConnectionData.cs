using GameLogic.Networking;
using GameServer;

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
    /// Represents the connection data of a player.
    /// </summary>
    /// <param name="Nickname">The nickname of the player.</param>
    /// <param name="Type">The type of the player.</param>
    /// <param name="EnumSerialization">The enum serialization format.</param>
    public record class Player(string Nickname, PlayerType Type, EnumSerializationFormat EnumSerialization)
        : ConnectionData(EnumSerialization);
}
