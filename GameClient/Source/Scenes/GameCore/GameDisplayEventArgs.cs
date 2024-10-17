using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the event arguments for the <see cref="Game"/> scene.
/// </summary>
/// <param name="joinCode">The join code to join the game.</param>
/// <param name="isSpectator">A value indicating whether the player is a spectator.</param>
internal class GameDisplayEventArgs(string? joinCode, bool isSpectator) : SceneDisplayEventArgs(false)
{
    /// <summary>
    /// Gets the join code to join the game.
    /// </summary>
    public string? JoinCode { get; } = joinCode;

    /// <summary>
    /// Gets a value indicating whether the player is a spectator.
    /// </summary>
    public bool IsSpectator { get; } = isSpectator;
}
