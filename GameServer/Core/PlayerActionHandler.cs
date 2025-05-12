using GameLogic;
using Serilog;

namespace GameServer;

/// <summary>
/// Handles movement and ability-related player actions.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger instance.</param>
internal sealed class PlayerActionHandler(GameInstance game, ILogger logger)
{
    /// <summary>
    /// Processes a movement action.
    /// </summary>
    /// <param name="player">The player connection.</param>
    /// <param name="direction">The movement direction.</param>
    public void HandleMovement(PlayerConnection player, MovementDirection direction)
    {
        logger.Verbose("Trying to move the tank of {player} {direction}.", player, direction);
        game.Systems.Movement.TryMoveTank(player.Instance.Tank, direction);
    }

    /// <summary>
    /// Processes a tank and/or turret rotation.
    /// </summary>
    /// <param name="player">The player connection.</param>
    /// <param name="tankRotation">The optional tank rotation.</param>
    /// <param name="turretRotation">The optional turret rotation.</param>
    public void HandleRotation(PlayerConnection player, Rotation? tankRotation, Rotation? turretRotation)
    {
        if (tankRotation is not null)
        {
            game.Systems.Rotation.TryRotateTank(player.Instance.Tank, tankRotation.Value);
        }

        if (turretRotation is not null)
        {
            game.Systems.Rotation.TryRotateTurret(player.Instance.Tank.Turret, turretRotation.Value);
        }
    }

    /// <summary>
    /// Gets the ability action delegate based on player tank type.
    /// </summary>
    /// <param name="type">The ability type.</param>
    /// <param name="player">The player owning the tank.</param>
    /// <returns>The action delegate, or null if unsupported.</returns>
    public Func<dynamic?>? GetAbilityAction(AbilityType type, Player player)
    {
        return type switch
        {
            AbilityType.FireBullet
                => () => game.Systems.Bullet.TryFireBullet(player.Tank.Turret.Bullet!, BulletType.Basic),
#if !STEREO
            AbilityType.UseRadar
                => () => game.Systems.Radar.TryUseRadar(player.Tank.Radar!),

            AbilityType.DropMine
                => () => game.Systems.Mine.TryDropMine(player.Tank.Mine!),

            AbilityType.FireDoubleBullet
                => () => game.Systems.Bullet.TryFireBullet(player.Tank.Turret.DoubleBullet!, BulletType.Double),

            AbilityType.UseLaser
                => () => game.Systems.Laser.TryUseLaser(player.Tank.Turret.Laser!),

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid ability type"),
#else
            AbilityType.UseRadar when player.Tank is LightTank light
                => () => game.Systems.Radar.TryUseRadar(light.Radar!),

            AbilityType.DropMine when player.Tank is HeavyTank heavy
                => () => game.Systems.Mine.TryDropMine(heavy.Mine!),

            AbilityType.FireDoubleBullet when player.Tank.Turret is LightTurret light
                => () => game.Systems.Bullet.TryFireBullet(light.DoubleBullet!, BulletType.Double),

            AbilityType.UseLaser when player.Tank.Turret is HeavyTurret heavy
                => () => game.Systems.Laser.TryUseLaser(heavy.Laser!),

            AbilityType.FireHealingBullet when player.Tank.Turret is Turret turret
                => () => game.Systems.Bullet.TryFireBullet(turret.HealingBullet!, BulletType.Healing),

            AbilityType.FireStunBullet when player.Tank.Turret is Turret turret
                => () => game.Systems.Bullet.TryFireBullet(turret.StunBullet!, BulletType.Stun),

            _ => null,
#endif
        };
    }

#if DEBUG && STEREO

    /// <summary>
    /// Instantly regenerates the specified ability of a player to its fully usable state.
    /// </summary>
    /// <param name="player">The player whose ability should be regenerated.</param>
    /// <param name="type">The type of ability to regenerate.</param>
    public void FullyRegenerateAbility(PlayerConnection player, AbilityType type)
    {
        var tank = player.Instance.Tank;

        IRegenerable? ability = type switch
        {
            AbilityType.FireBullet => tank.Turret.Bullet,
            AbilityType.FireDoubleBullet when tank.Turret is LightTurret light => light.DoubleBullet,
            AbilityType.UseLaser when tank.Turret is HeavyTurret heavy => heavy.Laser,
            AbilityType.UseRadar when tank is LightTank light => light.Radar,
            AbilityType.DropMine when tank is HeavyTank heavy => heavy.Mine,
            AbilityType.FireHealingBullet => tank.Turret.HealingBullet,
            AbilityType.FireStunBullet => tank.Turret.StunBullet,
            _ => null,
        };

        ability?.RegenerateFull();
    }

#endif
}
