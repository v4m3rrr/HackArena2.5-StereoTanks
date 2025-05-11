namespace GameLogic;

/// <summary>
/// Responsible for creating fully initialized tank instances with their turrets and abilities.
/// </summary>
/// <param name="stunSystem">The stun system for managing stun effects.</param>
/// <param name="healSystem">The heal system for managing healing effects.</param>
/// <param name="turretFactory">The factory for creating turrets.</param>
internal sealed class TankFactory(StunSystem stunSystem, HealSystem healSystem, TurretFactory turretFactory)
{
#if !STEREO

    /// <summary>
    /// Gets the item drop system.
    /// </summary>
    public required ItemDropSystem ItemDropSystem { private get; init; }

#endif

#if !STEREO

    /// <summary>
    /// Creates a tank instance with a turret and abilities.
    /// </summary>
    /// <param name="x">The x coordinate to spawn the tank.</param>
    /// <param name="y">The y coordinate to spawn the tank.</param>
    /// <param name="direction">The facing direction of the tank.</param>
    /// <param name="turretDirection">The direction of the turret.</param>
    /// <param name="owner">The owner of the tank.</param>
    /// <returns>A fully initialized <see cref="Tank"/> instance.</returns>
    public Tank Create(int x, int y, Direction direction, Direction turretDirection, Player owner)
    {
        var tank = new Tank(x, y, direction, owner);
        var turret = turretFactory.Create(turretDirection);

        turret.Tank = tank;
        tank.Turret = turret;

        tank.Dying += (s, e) =>
        {
            this.ItemDropSystem.TryDropItem(tank);
            healSystem.ClearFractionalBuffer(tank);
        };

        tank.Radar = new RadarAbility(stunSystem)
        {
            Tank = tank,
        };

        tank.Mine = new MineAbility(stunSystem)
        {
            Tank = tank,
        };

        return tank;
    }

#else
    /// <summary>
    /// Creates a light tank instance with a light turret.
    /// </summary>
    /// <param name="x">The x coordinate to spawn the tank.</param>
    /// <param name="y">The y coordinate to spawn the tank.</param>
    /// <param name="direction">The facing direction of the tank.</param>
    /// <param name="turretDirection">The direction of the turret.</param>
    /// <param name="owner">The owner of the tank.</param>
    /// <returns>A fully initialized <see cref="LightTank"/> instance.</returns>
    public LightTank CreateLightTank(int x, int y, Direction direction, Direction turretDirection, Player owner)
    {
        var tank = new LightTank(x, y, direction, owner);
        var turret = turretFactory.CreateLightTurret(turretDirection);

        turret.Tank = tank;
        tank.Turret = turret;

        tank.Dying += (s, e) => healSystem.ClearFractionalBuffer(tank);

        tank.Radar = new RadarAbility(stunSystem)
        {
            Tank = tank,
        };

        return tank;
    }

    /// <summary>
    /// Creates a heavy tank instance with a heavy turret.
    /// </summary>
    /// <param name="x">The x coordinate to spawn the tank.</param>
    /// <param name="y">The y coordinate to spawn the tank.</param>
    /// <param name="direction">The facing direction of the tank.</param>
    /// <param name="turretDirection">The direction of the turret.</param>
    /// <param name="owner">The owner of the tank.</param>
    /// <returns>A fully initialized <see cref="HeavyTank"/> instance.</returns>
    public HeavyTank CreateHeavyTank(int x, int y, Direction direction, Direction turretDirection, Player owner)
    {
        var tank = new HeavyTank(x, y, direction, owner);
        var turret = turretFactory.CreateHeavyTurret(turretDirection);

        turret.Tank = tank;
        tank.Turret = turret;

        tank.Dying += (s, e) => healSystem.ClearFractionalBuffer(tank);

        tank.Mine = new MineAbility(stunSystem)
        {
            Tank = tank,
        };

        return tank;
    }

    /// <summary>
    /// Creates a generic tank by type.
    /// </summary>
    /// <param name="type">The type of tank to create.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="direction">The body direction.</param>
    /// <param name="turretDirection">The turret direction.</param>
    /// <param name="owner">The owning player.</param>
    /// <returns>A tank of the requested type, or <see langword="null"/> if unsupported.</returns>
    public Tank? CreateTankByType(TankType type, int x, int y, Direction direction, Direction turretDirection, Player owner)
    {
        return type switch
        {
            TankType.Light => this.CreateLightTank(x, y, direction, turretDirection, owner),
            TankType.Heavy => this.CreateHeavyTank(x, y, direction, turretDirection, owner),
            _ => null,
        };
    }

#endif
}
