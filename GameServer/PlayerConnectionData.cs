using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the connection data of a player.
/// </summary>
/// <param name="Nickname">The nickname of the player.</param>
/// <param name="type">The type of the player.</param>
/// <param name="TypeOfPacketType">The type of packet type to use for the player.</param>
/// <param name="QuickJoin">A value indicating whether the player joined the game by quick join.</param>
public record struct PlayerConnectionData(
#if DEBUG
    string Nickname, PlayerType Type, TypeOfPacketType TypeOfPacketType, bool QuickJoin);
#else
    string Nickname, PlayerType Type, TypeOfPacketType TypeOfPacketType);
#endif
