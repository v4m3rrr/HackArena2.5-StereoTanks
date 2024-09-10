using static GameLogic.ZoneStatus;

namespace GameLogic;

/// <summary>
/// Represents a zone.
/// </summary>
public class Zone : IEquatable<Zone>
{
    /// <summary>
    /// The number of ticks required to capture the zone.
    /// </summary>
    public const int TicksToCapture = 100;

    private int remainingTicksToCapture = TicksToCapture;

    /// <summary>
    /// Initializes a new instance of the <see cref="Zone"/> class.
    /// </summary>
    /// <param name="x">The x-coordinate of the zone.</param>
    /// <param name="y">The y-coordinate of the zone.</param>
    /// <param name="width">The width of the zone.</param>
    /// <param name="height">The height of the zone.</param>
    /// <param name="index">The index of the zone.</param>
    /// <remarks>
    /// The <see cref="Status"/> property is set to <see cref="Neutral"/>.
    /// </remarks>
    internal Zone(int x, int y, int width, int height, char index)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
        this.Index = index;
        this.Status = new Neutral();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Zone"/> class.
    /// </summary>
    /// <param name="x">The x-coordinate of the zone.</param>
    /// <param name="y">The y-coordinate of the zone.</param>
    /// <param name="width">The width of the zone.</param>
    /// <param name="height">The height of the zone.</param>
    /// <param name="index">The index of the zone.</param>
    /// <param name="status">The status of the zone.</param>
    internal Zone(int x, int y, int width, int height, char index, ZoneStatus status)
        : this(x, y, width, height, index)
    {
        this.Status = status;
    }

    /// <summary>
    /// Gets the x-coordinate of the zone.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the y-coordinate of the zone.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the width of the zone.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the zone.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the index of the zone.
    /// </summary>
    public char Index { get; }

    /// <summary>
    /// Gets the status of the zone.
    /// </summary>
    public ZoneStatus Status { get; internal set; }

    /// <summary>
    /// Determines whether the zone contains the specified point.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <returns>
    /// <see langword="true"/> if the zone contains the point;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ContainsPoint(int x, int y)
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
    public bool ContainsTank(Tank tank)
    {
        return this.ContainsPoint(tank.X, tank.Y);
    }

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
        return base.Equals(obj);
    }

    /// <inheritdoc cref="Equals(object?)"/>
    public bool Equals(Zone? other)
    {
        return other is not null
            && this.X == other.X
            && this.Y == other.Y
            && this.Width == other.Width
            && this.Height == other.Height
            && this.Index == other.Index;
    }

    /// <summary>
    /// Gets the hash code of the zone.
    /// </summary>
    /// <returns>The hash code of the zone.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y, this.Width, this.Height, this.Index);
    }

    /// <summary>
    /// Updates the capturing status of the zone.
    /// </summary>
    /// <param name="tanks">The tanks in the game.</param>
    internal void UpdateCapturingStatus(IEnumerable<Tank> tanks)
    {
        var tanksInZone = tanks.Where(this.ContainsTank).ToList();
        var tanksInZoneCount = tanksInZone.Count();

        switch (this.Status)
        {
            case Neutral:
                this.HandleNeutralState(tanksInZone);
                break;
            case BeingCaptured beingCaptured:
                this.HandleBeingCapturedState(beingCaptured, tanksInZone, tanksInZoneCount);
                break;
            case Captured captured:
                this.HandleCapturedState(captured, tanksInZone, tanksInZoneCount);
                break;
            case BeingContested beingContested:
                this.HandleBeingContestedState(beingContested, tanksInZone, tanksInZoneCount);
                break;
            case BeingRetaken beingRetaken:
                this.HandleBeingRetakenState(beingRetaken, tanksInZone, tanksInZoneCount);
                break;
            default:
                throw new InvalidOperationException($"Unknown status type: {this.Status.GetType().Name}");
        }
    }

    /// <summary>
    /// Handles the logic when the zone is in a neutral state.
    /// If only one tank is in the zone, it starts capturing the zone.
    /// </summary>
    private void HandleNeutralState(List<Tank> tanksInZone)
    {
        if (tanksInZone.Count == 1)
        {
            // A single tank starts capturing the neutral zone
            var tank = tanksInZone.First();
            this.Status = new BeingCaptured(tank.Owner, this.remainingTicksToCapture);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is being captured by a player.
    /// </summary>
    private void HandleBeingCapturedState(BeingCaptured beingCaptured, List<Tank> tanksInZone, int tanksInZoneCount)
    {
        if (tanksInZoneCount == 0)
        {
            // If no tanks remain in the zone, the capturing process reverses
            beingCaptured.RemainingTicks++;

            if (beingCaptured.RemainingTicks >= TicksToCapture)
            {
                // Zone goes back to neutral if enough time passes without a tank
                this.Status = new Neutral();
            }
        }
        else if (tanksInZoneCount == 1)
        {
            var tank = tanksInZone.First();
            if (tank == beingCaptured.Player.Tank)
            {
                // Continue capturing if the same player's tank remains in the zone
                beingCaptured.RemainingTicks--;

                if (beingCaptured.RemainingTicks <= 0)
                {
                    // Capture is complete
                    this.Status = new Captured(beingCaptured.Player);
                }
            }
            else
            {
                // Another tank is now capturing the zone
                this.Status = new BeingCaptured(tank.Owner, this.remainingTicksToCapture);
            }
        }
        else
        {
            // Multiple tanks in the zone, the zone is contested
            this.Status = new BeingContested(tanksInZone.Select(t => t.Owner), null);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is already captured by a player.
    /// </summary>
    private void HandleCapturedState(Captured captured, List<Tank> tanksInZone, int tanksInZoneCount)
    {
        bool ownerInZone = tanksInZone.Any(t => t.Owner == captured.Player);

        if (tanksInZoneCount == 0)
        {
            // Award points to the owner if no one is contesting
            captured.Player.Score++;
        }
        else if (tanksInZoneCount == 1)
        {
            var tank = tanksInZone.First();
            if (ownerInZone)
            {
                // The owner is still in the zone, continue awarding points and heal
                captured.Player.Score++;
                captured.Player.Tank.Heal(1);
            }
            else
            {
                // Another player tries to retake the zone
                this.Status = new BeingRetaken(captured.Player, tank.Owner, this.remainingTicksToCapture);
            }
        }
        else
        {
            // Multiple tanks are contesting the captured zone
            this.Status = new BeingContested(tanksInZone.Select(t => t.Owner), captured.Player);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is contested by multiple tanks.
    /// </summary>
    private void HandleBeingContestedState(BeingContested beingContested, List<Tank> tanksInZone, int tanksInZoneCount)
    {
        if (tanksInZoneCount == 0)
        {
            // If no tanks remain, return the zone to neutral or back to the previous owner
            this.Status = beingContested.CapturedBy is null
                ? new Neutral()
                : new Captured(beingContested.CapturedBy);
        }
        else if (tanksInZoneCount == 1)
        {
            var tank = tanksInZone.First();
            this.Status = tank.Owner == beingContested.CapturedBy
                ? new Captured(beingContested.CapturedBy)
                : beingContested.CapturedBy is null
                    ? new BeingCaptured(tank.Owner, this.remainingTicksToCapture)
                    : new BeingRetaken(beingContested.CapturedBy, tank.Owner, this.remainingTicksToCapture);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is being retaken by another player.
    /// </summary>
    private void HandleBeingRetakenState(BeingRetaken beingRetaken, List<Tank> tanksInZone, int tanksInZoneCount)
    {
        beingRetaken.CapturedBy.Score++;

        if (tanksInZoneCount == 0)
        {
            // If no tanks remain, reverse the retake process
            beingRetaken.RemainingTicks++;

            if (beingRetaken.RemainingTicks >= TicksToCapture)
            {
                // Retake complete, the zone now belongs to the new player
                this.Status = new Captured(beingRetaken.CapturedBy);
            }
        }
        else if (tanksInZoneCount == 1)
        {
            var tank = tanksInZone.First();
            if (tank == beingRetaken.RetakenBy.Tank)
            {
                // Continue the retake if the retaking player's tank is still in the zone
                beingRetaken.RemainingTicks--;

                if (beingRetaken.RemainingTicks <= 0)
                {
                    // Retake is complete
                    this.Status = new Captured(beingRetaken.RetakenBy);
                }
            }
        }
        else
        {
            // The zone is contested during the retaking process
            this.Status = new BeingContested(tanksInZone.Select(t => t.Owner), beingRetaken.CapturedBy);
        }
    }
}
