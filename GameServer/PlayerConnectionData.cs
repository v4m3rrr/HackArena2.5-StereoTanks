using GameLogic.Networking;

namespace GameServer;

/// <summary>
/// Represents the connection data of a player.
/// </summary>
/// <param name="Nickname">The nickname of the player.</param>
/// <param name="TypeOfPacketType">The type of packet type to use for the player.</param>
/// <param name="quickJoin">A value indicating whether the player joined the game by quick join.</param>
public record struct PlayerConnectionData(
#if DEBUG
    string Nickname, TypeOfPacketType TypeOfPacketType, bool quickJoin);
#else
    string Nickname, TypeOfPacketType TypeOfPacketType);
#endif
