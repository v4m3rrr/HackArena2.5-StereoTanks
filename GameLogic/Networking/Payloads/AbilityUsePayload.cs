namespace GameLogic.Networking;

/// <summary>
/// Represents an ability use payload.
/// </summary>
/// <param name="AbilityType">The ability type.</param>
public record class AbilityUsePayload(AbilityType AbilityType) : IPacketPayload, IActionPayload
{
    /// <inheritdoc/>
    public PacketType Type => PacketType.AbilityUse;

    /// <inheritdoc/>
    public string? GameStateId { get; init; }
}
