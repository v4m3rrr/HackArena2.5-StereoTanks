namespace GameLogic;

/// <summary>
/// Represents a player.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Player"/> class.
/// </remarks>
/// <param name="id">The id of the player.</param>
public class Player(string id) : IEquatable<Player>
{
#if !STEREO
    private uint color;
    private string nickname = null!;
#endif

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

    /// <summary>
    /// Gets or sets the tank of the player.
    /// </summary>
    public Tank Tank { get; set; } = default!;

#if STEREO

    /// <summary>
    /// Gets the team of the player.
    /// </summary>
    public Team Team { get; internal set; } = null!;

#endif

#if CLIENT

    /// <summary>
    /// Gets or sets the remaining regeneration ticks of the player tank.
    /// </summary>
    /// <remarks>
    /// It is used to display the regeneration progress of the tank.
    /// </remarks>
    public int? RemainingRespawnTankTicks { get; set; }

    /// <summary>
    /// Gets a value indicating whether the player tank is dead.
    /// </summary>
    public bool IsTankDead => this.RemainingRespawnTankTicks is not null;

    /// <summary>
    /// Gets the regeneration progress of the tank.
    /// </summary>
    /// <remarks>
    /// It is used to display the regeneration progress of the tank.
    /// </remarks>
    public float? RespawnTankProgress => RegenerationUtils.GetRegenerationProgres(
        this.RemainingRespawnTankTicks, Tank.RegenerationTicks);

#endif

#if !STEREO

    /* Backward compatibility */

#pragma warning disable SA1201

    private bool? isUsingRadar;
    private bool[,]? visibilityGrid;

    /// <summary>
    /// Gets the visibility grid of the player.
    /// </summary>
    internal bool? IsUsingRadar
    {
        get => this.Tank?.Radar?.IsActive ?? this.isUsingRadar;
        init => this.isUsingRadar = value;
    }

    /// <summary>
    /// Gets the visibility grid of the player.
    /// </summary>
    internal bool[,]? VisibilityGrid
    {
        get => this.Tank?.VisibilityGrid ?? this.visibilityGrid;
        init => this.visibilityGrid = value;
    }

#endif

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Player);
    }

    /// <inheritdoc/>
    public bool Equals(Player? other)
    {
        return this.Id == other?.Id;
    }

    /// <inheritdoc/>
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

#if !STEREO
        /* Backward compatibility */
        this.visibilityGrid = player.VisibilityGrid;
#endif

        this.RemainingRespawnTankTicks = player.RemainingRespawnTankTicks;

        if (player.Tank is not null)
        {
            this.Tank?.UpdateFrom(player.Tank);
        }
    }

#endif
}
