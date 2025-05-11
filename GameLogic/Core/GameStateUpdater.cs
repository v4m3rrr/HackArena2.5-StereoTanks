namespace GameLogic;

/// <summary>
/// Updates the game state by invoking relevant systems per tick.
/// </summary>
/// <param name="systems">The game systems.</param>
internal sealed class GameStateUpdater(GameSystems systems)
{
    /// <summary>
    /// Performs a full game state update for the current tick.
    /// </summary>
    /// <param name="tickDeltaTime">The tick time elapsed since the last update.</param>
    public void Update(float tickDeltaTime)
    {
        systems.Stun.Update();

        systems.AbilityMaintenance.UpdateCooldowns();
        systems.TankRegeneration.Update();
        systems.Bullet.Update(tickDeltaTime);
        systems.Mine.Update();
        systems.Laser.Update();
        systems.Zone.Update();
        systems.Visibility.Update();

#if !STEREO
        systems.ItemPickup.Update();
        systems.ItemSpawn.GenerateNewItem();
#endif
    }

    /// <summary>
    /// Resets the abilities of all tanks and turrets in the grid.
    /// </summary>
    public void ResetAbilities()
    {
        systems.AbilityMaintenance.Reset();
    }
}
