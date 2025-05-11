namespace GameLogic;

/// <summary>
/// Handles spawning of tanks on the map.
/// </summary>
/// <param name="grid">The grid where tanks are spawned.</param>
/// <param name="visibilitySystem">The visibility system used to check spawn positions.</param>
/// <param name="tankFactory">The factory used to create tanks.</param>
internal sealed class SpawnSystem(Grid grid, VisibilitySystem visibilitySystem, TankFactory tankFactory)
{
#if !STEREO

    /// <summary>
    /// Generates a tank and spawns it at a random valid position.
    /// </summary>
    /// <param name="owner">The player that owns the tank.</param>
    /// <returns>The generated tank.</returns>
    public Tank GenerateTank(Player owner)
    {
        var (x, y) = this.GetFreeSpawnPosition();

        var direction = EnumUtils.Random<Direction>(grid.Random);
        var turretDir = EnumUtils.Random<Direction>(grid.Random);

        var tank = tankFactory.Create(x, y, direction, turretDir, owner)!;

        tank.Regenerated += (s, e) =>
        {
            var (sx, sy) = this.GetFreeSpawnPosition();
            tank.SetPosition(sx, sy);
        };

        owner.Tank = tank;
        grid.Tanks.Add(tank);

        return tank;
    }

#else

    /// <summary>
    /// Creates a new <see cref="DeclaredTankStub"/> for the specified player and tank type.
    /// </summary>
    /// <param name="owner">The player who will own the tank.</param>
    /// <param name="type">The type of tank to declare.</param>
    /// <returns>The declared tank stub.</returns>
    public static DeclaredTankStub GenerateDeclaredTank(Player owner, TankType type)
    {
        return new DeclaredTankStub(owner, type);
    }

    /// <summary>
    /// Generates a tank based on a declared stub (used in pre-match declarations).
    /// </summary>
    /// <param name="stub">The stub containing the owner and tank type.</param>
    /// <returns>The generated tank.</returns>
    public Tank GenerateTank(DeclaredTankStub stub)
    {
        return this.GenerateTank(stub.Owner, stub.Type);
    }

    /// <summary>
    /// Generates a tank and spawns it at a random valid position.
    /// </summary>
    /// <param name="owner">The player that owns the tank.</param>
    /// <param name="type">The tank type.</param>
    /// <returns>The generated tank.</returns>
    public Tank GenerateTank(Player owner, TankType type)
    {
        var (x, y) = this.GetFreeSpawnPosition();

        var direction = EnumUtils.Random<Direction>(grid.Random);
        var turretDir = EnumUtils.Random<Direction>(grid.Random);

        var tank = tankFactory.CreateTankByType(type, x, y, direction, turretDir, owner)!;

        tank.Regenerated += (s, e) =>
        {
            var (sx, sy) = this.GetFreeSpawnPosition();
            tank.SetPosition(sx, sy);
        };

        owner.Tank = tank;
        grid.Tanks.Add(tank);

        return tank;
    }

#endif

    private (int X, int Y) GetFreeSpawnPosition()
    {
        int attempts = 0;
        int x, y;

        do
        {
            x = grid.Random.Next(grid.Dim);
            y = grid.Random.Next(grid.Dim);

            if (attempts++ >= 1000)
            {
                break;
            }
        } while (grid.GetCellObjects(x, y).Any()
            || grid.Zones.Any(z => z.Contains(x, y))
            || visibilitySystem.IsVisibleByAnyTank(x, y));

        return (x, y);
    }
}
