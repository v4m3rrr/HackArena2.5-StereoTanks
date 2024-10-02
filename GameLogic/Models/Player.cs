namespace GameLogic;

/// <summary>
/// Represents a player.
/// </summary>
public class Player : IEquatable<Player>
{
    private const int RegenTicks = 50;
    private Tank tank;

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="id">The id of the player.</param>
    /// <param name="nickname">The nickname of the player.</param>
    /// <param name="color">The color of the player.</param>
    /// <remarks>
    /// <para>
    /// The <see cref="Tank"/> property is set to <see langword="null"/>.
    /// See its documentation for more information.
    /// </para>
    /// </remarks>
    public Player(string id, string nickname, uint color)
    {
        this.Id = id;
        this.Nickname = nickname;
        this.Color = color;
        this.tank = null!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="id">The id of the player.</param>
    /// <param name="nickname">The nickname of the player.</param>
    /// <param name="color">The color of the player.</param>
    /// <param name="remainingTicksToRegen">The remaining ticks to regenerate the tank.</param>
    /// <param name="visibilityGrid">The visibility grid of the player.</param>
    /// <remarks>
    /// The <see cref="Tank"/> property is set to <see langword="null"/>.
    /// See its documentation for more information.
    /// </remarks>
    public Player(string id, string nickname, uint color, int? remainingTicksToRegen, bool[,]? visibilityGrid)
        : this(id, nickname, color)
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
    public string Id { get; private set; }

    /// <summary>
    /// Gets the nickname of the player.
    /// </summary>
    public string Nickname { get; private set; }

    /// <summary>
    /// Gets the score of the player.
    /// </summary>
    public int Score { get; internal set; } = 0;

    /// <summary>
    /// Gets the color of the player.
    /// </summary>
    public uint Color { get; internal set; }

    /// <summary>
    /// Gets or sets the ping of the player.
    /// </summary>
    public int Ping { get; set; }

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
            this.Tank.Died += (s, e) => this.RemainingTicksToRegen = RegenTicks;
        }
    }

    /// <summary>
    /// Gets the visibility grid of the player.
    /// </summary>
    public bool[,]? VisibilityGrid { get; private set; }

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

    /// <summary>
    /// Updates the player from another player.
    /// </summary>
    /// <param name="player">The player to update from.</param>
    public void UpdateFrom(Player player)
    {
        this.Nickname = player.Nickname;
        this.Color = player.Color;
        this.Score = player.Score;
        this.Ping = player.Ping;
        this.tank = player.tank;
        this.RemainingTicksToRegen = player.RemainingTicksToRegen;
        this.VisibilityGrid = player.VisibilityGrid;
    }

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
            this.Tank.Heal(100);
            this.RemainingTicksToRegen = null;
            this.TankRegenerated?.Invoke(this, EventArgs.Empty);
        }
    }
}
