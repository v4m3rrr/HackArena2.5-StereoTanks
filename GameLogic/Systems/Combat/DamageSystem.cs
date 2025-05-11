namespace GameLogic;

#pragma warning disable CS9113

/// <summary>
/// Handles applying damage to tanks and death-related effects.
/// </summary>
/// <param name="healSystem">The heal system.</param>
internal sealed class DamageSystem(HealSystem healSystem)
{
    /// <summary>
    /// Applies damage to a tank.
    /// </summary>
    /// <param name="target">The tank to damage.</param>
    /// <param name="amount">The amount of damage to apply.</param>
    /// <param name="source">The attacking player (optional).</param>
    /// <returns>The actual amount of damage taken.</returns>
    public int ApplyDamage(Tank target, int amount, Player? source = null)
    {
        if (target.IsDead || amount <= 0 || target.Health is null)
        {
            return 0;
        }

        var dealt = Math.Min(target.Health.Value, amount);
        target.Health = target.Health.Value - amount;

        if (target.Health <= 0)
        {
            target.OnDying(EventArgs.Empty);
            target.SetPosition(-1, -1);

            target.Health = 0;
            target.OnDied(EventArgs.Empty);

            if (source is not null && !target.Owner.Equals(source))
            {
                source.Kills++;

#if !STEREO
                healSystem.Heal(source.Tank, dealt);
#endif
            }
        }

        return dealt;
    }
}
