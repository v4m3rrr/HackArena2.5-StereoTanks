using System.Collections.Generic;
using System.Numerics;

namespace GameLogic;

/// <summary>
/// Handles health regeneration for tanks.
/// </summary>
internal class HealSystem
{
    private readonly Dictionary<Tank, float> fractionalHealBuffer = [];

    /// <summary>
    /// Removes any accumulated fractional healing for the specified tank.
    /// </summary>
    /// <param name="tank">The tank whose healing buffer should be cleared.</param>
    public void ClearFractionalBuffer(Tank tank)
    {
        _ = this.fractionalHealBuffer.Remove(tank);
    }

    /// <summary>
    /// Heals a tank by the specified number of integer health points.
    /// </summary>
    /// <param name="tank">The tank to heal.</param>
    /// <param name="points">The number of whole health points to restore.</param>
    public void Heal(Tank tank, int points)
    {
        ValidatePoints(points);

        if (tank.IsDead)
        {
            return;
        }

        tank.Health = Math.Clamp(tank.Health!.Value + points, 0, Tank.HealthMax);
    }

    /// <summary>
    /// Heals a tank by a fractional amount of health points.
    /// </summary>
    /// <param name="tank">The tank to heal.</param>
    /// <param name="points">The fractional amount of health points to restore.</param>
    /// <remarks>
    /// Fractional values are buffered and applied once they accumulate to at least 1 point.
    /// </remarks>
    public void Heal(Tank tank, float points)
    {
        ValidatePoints(points);

        if (tank.IsDead)
        {
            return;
        }

        if (!this.fractionalHealBuffer.TryAdd(tank, points))
        {
            this.fractionalHealBuffer[tank] += points;
        }

        int pointsToApply = (int)this.fractionalHealBuffer[tank];

        if (pointsToApply >= 1)
        {
            this.Heal(tank, pointsToApply);
            this.fractionalHealBuffer[tank] -= pointsToApply;
        }
    }

    private static void ValidatePoints<T>(T points)
        where T : INumber<T>
    {
        if (points < T.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(points), points, "Must be greater than or equal to 0.");
        }
    }
}
