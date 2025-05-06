namespace GameLogic;

/// <summary>
/// Represents a player.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Player"/> class.
/// </remarks>
/// <param name="id">The id of the player.</param>
/// <remarks>
/// <para>
/// The <see cref="Tank"/> property is set to <see langword="null"/>.
/// See its documentation for more information.
/// </para>
/// </remarks>
public class Player(string id) : IEquatable<Player>
{
    private const int RegenTicks = 50;
    private Tank tank = null!;

#if !STEREO
    private uint color;
    private string nickname = null!;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="id">The id of the player.</param>
    /// <param name="remainingTicksToRegen">The remaining ticks to regenerate the tank.</param>
    /// <param name="visibilityGrid">The visibility grid of the player.</param>
    internal Player(string id, int? remainingTicksToRegen, bool[,]? visibilityGrid)
        : this(id)
    {
        this.RemainingTicksToRegen = remainingTicksToRegen;
        this.VisibilityGrid = visibilityGrid;
    }

    /// <summary>
    /// Occurs when the tank regenerates.
    /// </summary>
    internal event EventHandler? TankRegenerated;

    /// <summary>
    /// Gets the id of the player.
    /// </summary>
    public string Id { get; private set; } = id;

#if !STEREO

    /// <summary>
    /// Gets the nickname of the player.
    /// </summary>
    public required string Nickname
    {
        get => this.nickname;
        init => this.nickname = value;
    }

    /// <summary>
    /// Gets the score of the player.
    /// </summary>
    public int Score { get; internal set; } = 0;

#endif

#if STEREO

    /// <summary>
    /// Gets the color of the player.
    /// </summary>
    /// <remarks>
    /// The color is the same as the team color.
    /// </remarks>
    public uint Color => this.Team.Color;

#else

    /// <summary>
    /// Gets the color of the player.
    /// </summary>
    public required uint Color
    {
        get => this.color;
        init => this.color = value;
    }

#endif

    /// <summary>
    /// Gets the number of players killed by this player.
    /// </summary>
    public int Kills { get; internal set; } = 0;

    /// <summary>
    /// Gets or sets the ping of the player.
    /// </summary>
    public int Ping { get; set; }

#if !STEREO
    // TODO: Move it to tank class

    /// <summary>
    /// Gets a value indicating whether the player is using radar.
    /// </summary>
    public bool IsUsingRadar { get; internal set; }

#endif

    /// <summary>
    /// Gets the regeneration progress of the tank.
    /// </summary>
    /// <value>
    /// The regeneration progress of the tank as a value between 0 and 1.
    /// </value>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is not dead.
    /// </remarks>
    public float? RegenProgress => this.RemainingTicksToRegen is not null
        ? 1 - (this.RemainingTicksToRegen / (float)RegenTicks)
        : null;

    /// <summary>
    /// Gets the remaining ticks to regenerate the tank.
    /// </summary>
    /// <remarks>
    /// The value is <see langword="null"/> if the tank is not dead.
    /// </remarks>
    public int? RemainingTicksToRegen { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the player is dead.
    /// </summary>
    public bool IsDead => this.Tank is null || this.Tank.IsDead;

    /// <summary>
    /// Gets the tank of the player.
    /// </summary>
    /// <remarks>
    /// The setter is internal because the owner is set
    /// in the <see cref="Grid.UpdateFromGameStatePayload"/> method.
    /// </remarks>
    public Tank Tank
    {
        get => this.tank;
        internal set
        {
            this.tank = value;
            this.tank.Died += (s, e) => this.RemainingTicksToRegen = RegenTicks;
        }
    }

    /// <summary>
    /// Gets the visibility grid of the player.
    /// </summary>
    public bool[,]? VisibilityGrid { get; private set; }

#if STEREO

    /// <summary>
    /// Gets the team of the player.
    /// </summary>
    public Team Team { get; internal set; } = null!;

#endif

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is equal to the current object;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Player);
    }

    /// <inheritdoc cref="Equals(object)"/>
    /// <remarks>
    /// The players are considered equal if they have the same id.
    /// </remarks>
    public bool Equals(Player? other)
    {
        return this.Id == other?.Id;
    }

    /// <summary>
    /// Gets the hash code of the player.
    /// </summary>
    /// <returns>The hash code of the player.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id);
    }

#if CLIENT

    /// <summary>
    /// Updates the player from another player.
    /// </summary>
    /// <param name="player">The player to update from.</param>
    public void UpdateFrom(Player player)
    {
#if !STEREO
        this.color = player.Color;
        this.nickname = player.Nickname;
        this.Score = player.Score;
#endif
        this.Ping = player.Ping;
        this.RemainingTicksToRegen = player.RemainingTicksToRegen;
        this.VisibilityGrid = player.VisibilityGrid;

        if (player.Tank is not null)
        {
            this.tank?.UpdateFrom(player.Tank);
        }
    }

#endif

    /// <summary>
    /// Calculates the visibility grid for the player.
    /// </summary>
    /// <param name="calculator">The fog of war calculator to use.</param>
    internal void CalculateVisibilityGrid(FogOfWarManager calculator)
    {
        const int angle = 144;

        this.VisibilityGrid = this.IsDead
            ? calculator.EmptyGrid
            : calculator.CalculateVisibilityGrid(this.Tank, angle);
    }

    /// <summary>
    /// Regenerates the tank over time, if it is dead.
    /// </summary>
    internal void UpdateRegenerationProgress()
    {
        if (!this.Tank.IsDead)
        {
            return;
        }

        if (--this.RemainingTicksToRegen <= 0)
        {
            this.Tank.SetHealth(100);
            this.RemainingTicksToRegen = null;
            this.TankRegenerated?.Invoke(this, EventArgs.Empty);
        }
    }
}
