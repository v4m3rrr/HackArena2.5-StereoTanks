using GameLogic.Networking;
using GameServer;

/// <summary>
/// Represents the connection data.
/// </summary>
/// <param name="TypeOfPacketType">The type of packet type.</param>
internal record class ConnectionData(TypeOfPacketType TypeOfPacketType)
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
    /// <param name="TypeOfPacketType">The type of packet type.</param>
    public record class Player(string Nickname, PlayerType Type, TypeOfPacketType TypeOfPacketType)
        : ConnectionData(TypeOfPacketType);
}
