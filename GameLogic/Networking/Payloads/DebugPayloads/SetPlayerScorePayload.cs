namespace GameLogic.Networking;

#if DEBUG && !STEREO

/// <summary>
/// Represents a packet payload to set the score of a player.
/// </summary>
/// <param name="PlayerNick">The player nickname.</param>
/// <param name="Score">The score of the player to set.</param>
public record class SetPlayerScorePayload(string PlayerNick, int Score) : IPacketPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.SetPlayerScore;
}

#endif
