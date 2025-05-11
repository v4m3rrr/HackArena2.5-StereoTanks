namespace GameLogic.Networking;

/// <summary>
/// Represents a visibility payload for the grid.
/// </summary>
/// <param name="Grid">The visibility grid of the grid.</param>
internal record class VisibilityPayload(bool[,] Grid);
