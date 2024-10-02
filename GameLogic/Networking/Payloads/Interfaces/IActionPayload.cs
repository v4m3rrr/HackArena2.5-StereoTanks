namespace GameLogic.Networking;

/// <summary>
/// Represents an action payload.
/// </summary>
public interface IActionPayload : IPacketPayload
{
    /// <summary>
    /// Gets the game state packet id,
    /// that this action is associated with.
    /// </summary>
    public string? GameStateId { get; init; }
}
