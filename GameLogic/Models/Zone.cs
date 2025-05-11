using GameLogic.ZoneStates;

namespace GameLogic;

/// <summary>
/// Represents a zone.
/// </summary>
public class Zone(int x, int y, int width, int height, char index) : IEquatable<Zone>
{
    /// <summary>
    /// Gets the x-coordinate of the zone.
    /// </summary>
    public int X { get; } = x;

    /// <summary>
    /// Gets the y-coordinate of the zone.
    /// </summary>
    public int Y { get; } = y;

    /// <summary>
    /// Gets the width of the zone.
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// Gets the height of the zone.
    /// </summary>
    public int Height { get; } = height;

    /// <summary>
    /// Gets the index of the zone.
    /// </summary>
    public char Index { get; } = index;

    /// <summary>
    /// Gets or sets the state of the zone.
    /// </summary>
    public ZoneState State { get; set; } = new NeutralZoneState();

    /// <summary>
    /// Determines whether the zone contains the specified point.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <returns>
    /// <see langword="true"/> if the zone contains the point;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(int x, int y)
    {
        return x >= this.X && x < this.X + this.Width
            && y >= this.Y && y < this.Y + this.Height;
    }

    /// <summary>
    /// Determines whether the zone contains the specified tank.
    /// </summary>
    /// <param name="tank">The tank to check.</param>
    /// <returns>
    /// <see langword="true"/> if the zone contains the tank;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Tank tank)
    {
        return this.Contains(tank.X, tank.Y);
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        return this.Equals(other as Zone);
    }

    /// <inheritdoc/>
    public bool Equals(Zone? other)
    {
        return other is not null
            && this.X == other.X
            && this.Y == other.Y
            && this.Width == other.Width
            && this.Height == other.Height
            && this.Index == other.Index;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y, this.Width, this.Height, this.Index);
    }

    /// <summary>
    /// Updates this zone model from a snapshot instance.
    /// </summary>
    /// <param name="snapshot">The zone instance to copy state from.</param>
    public void UpdateFrom(Zone snapshot)
    {
        this.State = snapshot.State;
    }
}
