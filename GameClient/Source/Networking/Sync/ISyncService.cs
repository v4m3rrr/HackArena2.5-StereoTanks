namespace GameClient.Networking;

/// <summary>
/// Defines a contract for synchronizing client-side
/// visual elements with the current game logic state.
/// </summary>
internal interface ISyncService
{
    /// <summary>
    /// Synchronizes the state of all associated visual elements
    /// based on the current game logic.
    /// </summary>
    void Sync();
}
