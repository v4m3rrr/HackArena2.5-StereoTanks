namespace GameLogic.Networking.GoToElements;

#if STEREO && HACKATHON

/// <summary>
/// Represents the costs associated with different actions in the pathfinding algorithm.
/// </summary>
public class Costs
{
    /// <summary>
    /// Gets the cost associated with moving forward.
    /// </summary>
    public float Forward { get; init; } = 1f;

    /// <summary>
    /// Gets the cost associated with moving backward.
    /// </summary>
    public float Backward { get; init; } = 1.5f;

    /// <summary>
    /// Gets the cost associated with rotating left.
    /// </summary>
    public float Rotate { get; init; } = 1.5f;
}

#endif
