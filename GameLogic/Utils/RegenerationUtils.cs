namespace GameLogic;

/// <summary>
/// Provides utility methods for handling regeneration logic.
/// </summary>
internal static class RegenerationUtils
{
    /// <summary>
    /// Calculates the normalized regeneration progress of the specified regenerable entity.
    /// </summary>
    /// <param name="regenerable">The regenerable entity to evaluate.</param>
    /// <returns>
    /// A value between 0.0 and 1.0 representing regeneration progress,
    /// or <see langword="null"/> if regeneration is complete.
    /// </returns>
    public static float? GetRegenerationProgres(IRegenerable regenerable)
    {
        var total = regenerable.TotalRegenerationTicks;
        var remaining = regenerable.RemainingRegenerationTicks;
        return GetRegenerationProgres(remaining, total);
    }

    /// <summary>
    /// Calculates the normalized regeneration progress based on remaining and total ticks.
    /// </summary>
    /// <param name="remaining">
    /// The number of remaining ticks until regeneration is complete
    /// or <see langword="null"/> if completed.
    /// </param>
    /// <param name="total">The total number of ticks required to fully regenerate.</param>
    /// <returns>
    /// A value between 0.0 and 1.0 representing regeneration progress,
    /// or <see langword="null"/> if regeneration is complete.
    /// </returns>
    public static float? GetRegenerationProgres(int? remaining, int total)
    {
        return remaining is null
            ? null
            : 1f - (remaining / (float)total);
    }

    /// <summary>
    /// Decreases the remaining regeneration ticks by one.
    /// Sets the value to <see langword="null"/> if regeneration is completed.
    /// </summary>
    /// <param name="remainingTicks">
    /// A reference to the remaining ticks value to update.
    /// </param>
    /// <remarks>
    /// If the value is already <see langword="null"/>, it remains unchanged.
    /// </remarks>
    public static void UpdateRegenerationProcess(ref int? remainingTicks)
    {
        if (remainingTicks is not null && --remainingTicks <= 0)
        {
            remainingTicks = null;
        }
    }
}
