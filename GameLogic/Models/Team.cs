namespace GameLogic;

#if STEREO

#pragma warning disable IDE0290

/// <summary>
/// Represents a team.
/// </summary>
public class Team : IEquatable<Team>
{
    private readonly List<Player> players;

#if SERVER

    /// <summary>
    /// Initializes a new instance of the <see cref="Team"/> class.
    /// </summary>
    /// <param name="name">The name of the team.</param>
    /// <param name="color">The color of the team.</param>
    public Team(string name, uint color)
    {
        this.players = [];
        this.Name = name;
        this.Color = color;
    }

#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="Team"/> class.
    /// </summary>
    /// <param name="name">The name of the team.</param>
    /// <param name="color">The color of the team.</param>
    /// <param name="players">The players in the team.</param>
    internal Team(string name, uint color, List<Player> players)
    {
        this.players = players;
        this.Name = name;
        this.Color = color;
    }

    /// <summary>
    /// Gets the name of the team.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the color of the team.
    /// </summary>
    public uint Color { get; private set; }

    /// <summary>
    /// Gets the score of the team.
    /// </summary>
    public int Score { get; internal set; } = 0;

    /// <summary>
    /// Gets the players in the team.
    /// </summary>
    public IEnumerable<Player> Players => this.players;

    /// <summary>
    /// Gets the combined visibility grid for the team,
    /// merged from the visibility of all member tanks.
    /// </summary>
    public bool[,]? CombinedVisibilityGrid
    {
        get
        {
            bool[,]? grid = null;

            foreach (var player in this.players.ToList())
            {
                if (player.Tank.VisibilityGrid is null)
                {
                    continue;
                }

                if (grid is null)
                {
                    var dim = player.Tank.VisibilityGrid.GetLength(0);
                    grid = new bool[dim, dim];
                }

                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    for (int y = 0; y < grid.GetLength(1); y++)
                    {
                        grid[x, y] |= player.Tank.VisibilityGrid[x, y];
                    }
                }
            }

            return grid;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Team);
    }

    /// <inheritdoc/>
    public bool Equals(Team? other)
    {
        return this.Name == other?.Name;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name);
    }

#if CLIENT

    /// <summary>
    /// Updates the team with the given team data.
    /// </summary>
    /// <param name="team">The team data to update from.</param>
    public void UpdateFrom(Team team)
    {
        this.Name = team.Name;
        this.Color = team.Color;
        this.Score = team.Score;

        foreach (var updatedPlayer in team.players)
        {
            foreach (var player in this.Players.ToList())
            {
                if (player.Equals(updatedPlayer))
                {
                    player.UpdateFrom(updatedPlayer);
                    break;
                }

                if (this.players.All(p => !p.Equals(updatedPlayer)))
                {
                    this.players.Add(updatedPlayer);
                }
            }
        }
    }

#endif

#if SERVER

    /// <summary>
    /// Adds a player to the team.
    /// </summary>
    /// <param name="player">The player to add.</param>
    public void AddPlayer(Player player)
    {
        this.players.Add(player);
    }

    /// <summary>
    /// Removes a player from the team.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the player was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemovePlayer(Player player)
    {
        return this.players.Remove(player);
    }

#endif
}

#endif
