namespace GameClient;

/// <summary>
/// Provides functions for animations.
/// </summary>
internal static class Animations
{
    /// <summary>
    /// Eases in a value.
    /// </summary>
    /// <param name="t">The value to ease in.</param>
    /// <returns>The eased in value.</returns>
    public static float EaseIn(float t)
    {
        return t * t * t;
    }

    /// <summary>
    /// Eases out a value.
    /// </summary>
    /// <param name="t">The value to ease out.</param>
    /// <returns>The eased out value.</returns>
    public static float EaseOut(float t)
    {
        return 1 - EaseIn(1 - t);
    }

    /// <summary>
    /// Eases in and out a value.
    /// </summary>
    /// <param name="t">The value to ease in and out.</param>
    /// <returns>The eased in and out value.</returns>
    public static float EaseInOut(float t)
    {
        return t < 0.5f ? EaseIn(t * 2) / 2 : (EaseOut((t * 2) - 1) / 2) + 0.5f;
    }
}
