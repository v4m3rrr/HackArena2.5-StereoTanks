namespace GameLogic.Networking.GoToElements;

#if STEREO && HACKATHON

/// <summary>
/// Represents a custom penalty for pathfinding.
/// </summary>
/// <param name="X">The X coordinate of the penalty.</param>
/// <param name="Y">The Y coordinate of the penalty.</param>
/// <param name="Penalty">The penalty value associated with the coordinates.</param>
public record class CustomPenalty(int X, int Y, float Penalty);

#endif
