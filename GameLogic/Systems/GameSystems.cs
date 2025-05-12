namespace GameLogic;

/// <summary>
/// Represents the game systems responsible for managing various game mechanics.
/// </summary>
internal class GameSystems
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameSystems"/> class.
    /// </summary>
    /// <param name="grid">The grid representing the game world.</param>
    public GameSystems(Grid grid)
    {
        this.Heal = new HealSystem();
        this.Damage = new DamageSystem(this.Heal);
        this.Score = new ScoreSystem();
        this.Stun = new StunSystem();
        this.BulletCollision = new BulletCollisionSystem(grid, this.Damage, this.Heal, this.Score, this.Stun);
        this.Bullet = new BulletSystem(grid, this.BulletCollision);
        this.Visibility = new VisibilitySystem(grid);
        this.Zone = new ZoneSystem(grid, this.Score, this.Heal);

#if !STEREO
        this.ItemDrop = new ItemDropSystem(grid);
        this.ItemPickup = new ItemPickupSystem(grid);
        this.ItemSpawn = new ItemSpawnSystem(grid);
#endif

        this.TurretFactory = new TurretFactory(this.Stun);
        this.TankFactory = new TankFactory(this.Stun, this.Heal, this.TurretFactory)
        {
#if !STEREO
            ItemDropSystem = this.ItemDrop,
#endif
        };

        this.AbilityMaintenance = new AbilityMaintenanceSystem(grid);
        this.Despawn = new DespawnSystem(grid, this.Heal, this.Score, this.Zone)
        {
#if !STEREO
            ItemDropSystem = this.ItemDrop,
#endif
        };
        this.Mine = new MineSystem(grid, this.Damage, this.Score, this.Stun);
        this.Laser = new LaserSystem(grid, this.Damage, this.Score, this.Stun, this.Mine);
        this.Movement = new MovementSystem(grid, this.Stun);
        this.Radar = new RadarSystem();
        this.Rotation = new RotationSystem(this.Stun);
        this.Spawn = new SpawnSystem(grid, this.Visibility, this.TankFactory);
        this.TankRegeneration = new TankRegenerationSystem(grid);
    }

    /// <summary>
    /// Gets the ability maintenance system.
    /// </summary>
    public AbilityMaintenanceSystem AbilityMaintenance { get; }

    /// <summary>
    /// Gets the bullet system.
    /// </summary>
    public BulletSystem Bullet { get; }

    /// <summary>
    /// Gets the bullet collision system.
    /// </summary>
    public BulletCollisionSystem BulletCollision { get; }

    /// <summary>
    /// Gets the damage system.
    /// </summary>
    public DamageSystem Damage { get; }

    /// <summary>
    /// Gets the despawn system.
    /// </summary>
    public DespawnSystem Despawn { get; }

    /// <summary>
    /// Gets the heal system.
    /// </summary>
    public HealSystem Heal { get; }

#if !STEREO

    /// <summary>
    /// Gets the item drop system.
    /// </summary>
    public ItemDropSystem ItemDrop { get; }

    /// <summary>
    /// Gets the item pickup system.
    /// </summary>
    public ItemPickupSystem ItemPickup { get; }

    /// <summary>
    /// Gets the item spawn system.
    /// </summary>
    public ItemSpawnSystem ItemSpawn { get; }

#endif

    /// <summary>
    /// Gets the laser system.
    /// </summary>
    public LaserSystem Laser { get; }

    /// <summary>
    /// Gets the mine system.
    /// </summary>
    public MineSystem Mine { get; }

    /// <summary>
    /// Gets the movement system.
    /// </summary>
    public MovementSystem Movement { get; }

    /// <summary>
    /// Gets the radar system.
    /// </summary>
    public RadarSystem Radar { get; }

    /// <summary>
    /// Gets the rotation system.
    /// </summary>
    public RotationSystem Rotation { get; }

    /// <summary>
    /// Gets the score system.
    /// </summary>
    public ScoreSystem Score { get; }

    /// <summary>
    /// Gets the spawn system.
    /// </summary>
    public SpawnSystem Spawn { get; }

    /// <summary>
    /// Gets the item pickup system.
    /// </summary>
    public StunSystem Stun { get; }

    /// <summary>
    /// Gets the tank factory system.
    /// </summary>
    public TankFactory TankFactory { get; }

    /// <summary>
    /// Gets the tank regeneration system.
    /// </summary>
    public TankRegenerationSystem TankRegeneration { get; }

    /// <summary>
    /// Gets the turret factory system.
    /// </summary>
    public TurretFactory TurretFactory { get; }

    /// <summary>
    /// Gets the visibility system.
    /// </summary>
    public VisibilitySystem Visibility { get; }

    /// <summary>
    /// Gets the zone system.
    /// </summary>
    public ZoneSystem Zone { get; }
}
