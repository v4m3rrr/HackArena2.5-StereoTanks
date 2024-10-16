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
    string? GameStateId { get; init; }

    /// <summary>
    /// Validates the enums in the payload.
    /// </summary>
    /// <remarks>
    /// If the enums are invalid, the method raises the <see cref="ConvertEnumFailed"/> exception.
    /// </remarks>
    internal void ValidateEnums();
}
