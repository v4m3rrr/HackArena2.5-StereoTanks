namespace GameLogic.Networking;

#if DEBUG && STEREO

/// <summary>
/// Represents a packet payload to set the score of a team.
/// </summary>
/// <param name="TeamName">The name of the team.</param>
/// <param name="Score">The score of the team to set.</param>
public record class SetScorePayload(string TeamName, int Score) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.SetScore;
}

#elif DEBUG

/// <summary>
/// Represents a packet payload to set the score of a player.
/// </summary>
/// <param name="PlayerNick">The player nickname.</param>
/// <param name="Score">The score of the player to set.</param>
public record class SetScorePayload(string PlayerNick, int Score) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.SetPlayerScore;
}

#endif
