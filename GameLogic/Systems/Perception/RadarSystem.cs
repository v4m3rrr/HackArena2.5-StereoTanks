namespace GameLogic;

/// <summary>
/// Handles activation and effects of the radar ability.
/// </summary>
internal sealed class RadarSystem
{
    /// <summary>
    /// Attempts to activate the radar ability.
    /// </summary>
    /// <param name="radar">The radar ability to use.</param>
    /// <returns>
    /// <see langword="true"/> if the radar was successfully used;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryUseRadar(RadarAbility radar)
    {
        if (!radar.CanUse)
        {
            return false;
        }

        radar.Use();
        return true;
    }
}
