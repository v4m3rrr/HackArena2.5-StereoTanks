using Newtonsoft.Json;

namespace GameLogic;

/// <summary>
/// Represents a wall.
/// </summary>
public class Wall
{
    internal Wall(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    [JsonConstructor]
    private Wall()
    {
    }

    /// <summary>
    /// Gets the X position of the wall.
    /// </summary>
    [JsonIgnore]
    public int X { get; internal set; }

    /// <summary>
    /// Gets the Y position of the wall.
    /// </summary>
    [JsonIgnore]
    public int Y { get; internal set; }
}
