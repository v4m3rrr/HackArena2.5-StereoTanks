namespace GameLogic;

/// <summary>
/// Defines a contract for capturable zone states.
/// </summary>
internal interface ICaptureState
{
    /// <summary>
    /// Gets the player who is currently capturing the zone.
    /// </summary>
    Player BeingCapturedBy { get; }

    /// <summary>
    /// Gets or sets the number of ticks remaining for the capture to complete.
    /// </summary>
    int RemainingTicks { get; set; }
}
