namespace GameLogic;

/// <summary>
/// Handles regeneration of ability cooldowns for all tanks.
/// </summary>
/// <param name="grid">The grid containing all tanks with abilities.</param>
internal sealed class AbilityMaintenanceSystem(Grid grid)
{
    /// <summary>
    /// Updates cooldown timers for tank abilities and turret abilities.
    /// </summary>
    public void UpdateCooldowns()
    {
        foreach (var tank in grid.Tanks)
        {
            foreach (var ability in tank.GetAbilities().OfType<IRegenerable>())
            {
                ability.RegenerateTick();
            }

            foreach (var ability in tank.Turret.GetAbilities().OfType<IRegenerable>())
            {
                ability.RegenerateTick();
            }
        }
    }

    /// <summary>
    /// Resets all ability states for tanks and turrets.
    /// </summary>
    public void Reset()
    {
        foreach (Tank tank in grid.Tanks)
        {
            foreach (IAbility ability in tank.GetAbilities())
            {
                ability.Reset();
            }

            foreach (IAbility ability in tank.Turret.GetAbilities())
            {
                ability.Reset();
            }
        }
    }
}
