using GameLogic.Networking.Exceptions;

namespace GameLogic.Networking;

/// <summary>
/// Represents an action payload.
/// </summary>
public abstract class ActionPayload : IPacketPayload
{
    /// <inheritdoc/>
    public abstract PacketType Type { get; }

    /// <summary>
    /// Gets the game state packet id,
    /// that this action is associated with.
    /// </summary>
    public string? GameStateId { get; init; }

    /// <summary>
    /// Validates the payload.
    /// </summary>
    /// <remarks>
    /// If the payload is invalid, <see cref="PayloadEnumValidationError{T}"/> is thrown.
    /// </remarks>
    internal virtual void ValidateEnums()
    {
    }
}
