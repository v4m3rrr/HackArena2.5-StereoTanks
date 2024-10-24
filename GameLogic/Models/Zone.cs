using System.Diagnostics;
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
    public const int TicksToCapture = 50;

    private readonly Dictionary<Player, int> remainingTicksToCapture = [];
    private int updateCount;

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
        this.updateCount++;

        var tanksInZone = tanks.Where(this.ContainsTank).ToList();
        var tanksOutsideZone = tanks.Except(tanksInZone).ToList();

        switch (this.Status)
        {
            case Neutral:
                this.HandleNeutralState(tanksInZone);
                break;
            case BeingCaptured:
                this.HandleBeingCapturedState(tanksInZone);
                break;
            case Captured captured:
                this.HandleCapturedState(captured, tanksInZone);
                break;
            case BeingContested beingContested:
                this.HandleBeingContestedState(beingContested, tanksInZone);
                break;
            case BeingRetaken beingRetaken:
                this.HandleBeingRetakenState(beingRetaken, tanksInZone);
                break;
            default:
                throw new InvalidOperationException($"Unknown status type: {this.Status.GetType().Name}");
        }

        foreach (var player in tanksOutsideZone.Select(t => t.Owner))
        {
            if (this.remainingTicksToCapture.ContainsKey(player)
                && this.IncrementRemainingTicksToCapture(player) >= TicksToCapture)
            {
                this.RemovePlayerFromRemainingTicksToCapture(player);
            }
        }

        if (this.Status is BeingCaptured beingCaptured)
        {
            if (this.remainingTicksToCapture.Count == 0)
            {
                this.Status = new Neutral();
            }
            else
            {
                if (tanksInZone.Contains(beingCaptured.Player.Tank))
                {
                    this.Status = new BeingCaptured(beingCaptured.Player, this.GetRemainingTicksToCapture(beingCaptured.Player));
                }
                else
                {
                    var playerClosestToCapture = this.remainingTicksToCapture.OrderBy(kvp => kvp.Value).First();
                    this.Status = new BeingCaptured(playerClosestToCapture.Key, playerClosestToCapture.Value);
                }
            }
        }

        if (this.Status is BeingRetaken beingRetaken1)
        {
            if (this.remainingTicksToCapture.Count == 0)
            {
                this.Status = new Captured(beingRetaken1.CapturedBy);
            }
            else
            {
                if (tanksInZone.Contains(beingRetaken1.RetakenBy.Tank))
                {
                    this.Status = new BeingRetaken(beingRetaken1.CapturedBy, beingRetaken1.RetakenBy, this.GetRemainingTicksToCapture(beingRetaken1.RetakenBy));
                }
                else
                {
                    var playerClosestToCapture = this.remainingTicksToCapture.Where(x => x.Key != beingRetaken1.CapturedBy).OrderBy(kvp => kvp.Value).First();
                    this.Status = new BeingRetaken(beingRetaken1.CapturedBy, playerClosestToCapture.Key, playerClosestToCapture.Value);
                }
            }
        }
    }

    /// <summary>
    /// Handles the zone logic when a player is removed from the game.
    /// </summary>
    /// <param name="player">The player that was removed from the game.</param>
    /// <param name="players">The players in the game.</param>
    internal void HandlePlayerRemoved(Player player, IEnumerable<Player> players)
    {
        this.RemovePlayerFromRemainingTicksToCapture(player);

        switch (this.Status)
        {
            case BeingCaptured beingCaptured when beingCaptured.Player == player:
            case Captured captured when captured.Player == player:
                this.Status = new Neutral();
                break;

            case BeingContested beingContested when beingContested.CapturedBy == player:
                this.Status = new BeingContested(null);
                break;

            case BeingRetaken beingRetaken when beingRetaken.CapturedBy == player:
                this.Status = players.Contains(beingRetaken.RetakenBy)
                    ? new BeingCaptured(
                        beingRetaken.RetakenBy,
                        this.GetRemainingTicksToCapture(beingRetaken.RetakenBy))
                    : new Neutral();
                break;

            case BeingRetaken beingRetaken when beingRetaken.RetakenBy == player:
                this.Status = players.Contains(beingRetaken.CapturedBy)
                    ? new Captured(beingRetaken.CapturedBy)
                    : new Neutral();
                break;
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
            var tank = tanksInZone.First();
            this.remainingTicksToCapture[tank.Owner] = TicksToCapture;
            this.Status = new BeingCaptured(tank.Owner, TicksToCapture);
        }
        else if (tanksInZone.Count >= 2)
        {
            this.Status = new BeingContested(null);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is being captured by a player.
    /// </summary>
    private void HandleBeingCapturedState(List<Tank> tanksInZone)
    {
        if (tanksInZone.Count == 1)
        {
            var tank = tanksInZone.First();
            var remainingTicks = this.DecrementRemainingTicksToCapture(tank.Owner);

            if (remainingTicks == 0)
            {
                this.Status = new Captured(tank.Owner);
                this.remainingTicksToCapture.Clear();
                return;
            }

            this.Status = new BeingCaptured(tank.Owner, remainingTicks);
        }
        else if (tanksInZone.Count >= 2)
        {
            this.Status = new BeingContested(null);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is already captured by a player.
    /// </summary>
    private void HandleCapturedState(Captured captured, List<Tank> tanksInZone)
    {
        bool ownerInZone = tanksInZone.Any(t => t.Owner == captured.Player);

        if (tanksInZone.Count == 0)
        {
            if (this.updateCount % 2 == 0)
            {
                captured.Player.Score++;
            }
        }
        else if (tanksInZone.Count == 1)
        {
            var tank = tanksInZone.First();
            if (ownerInZone)
            {
                if (this.updateCount % 2 == 0)
                {
                    captured.Player.Score++;
                }

                if (this.updateCount % 4 == 0 && tank.Health < 80)
                {
                    captured.Player.Tank.Heal(1);
                }
            }
            else
            {
                var remainingTicks = this.remainingTicksToCapture[tank.Owner] = TicksToCapture;
                this.Status = new BeingRetaken(captured.Player, tank.Owner, remainingTicks);
            }
        }
        else
        {
            this.Status = new BeingContested(captured.Player);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is contested by multiple tanks.
    /// </summary>
    private void HandleBeingContestedState(BeingContested beingContested, List<Tank> tanksInZone)
    {
        if (tanksInZone.Count == 0)
        {
            this.Status = beingContested.CapturedBy is null
                ? new Neutral()
                : new Captured(beingContested.CapturedBy);
        }
        else if (tanksInZone.Count == 1)
        {
            var tank = tanksInZone.First();
            var remainingTicks = this.GetRemainingTicksToCapture(tank.Owner);
            this.Status = tank.Owner == beingContested.CapturedBy
                ? new Captured(beingContested.CapturedBy)
                : beingContested.CapturedBy is null
                    ? new BeingCaptured(tank.Owner, remainingTicks)
                    : new BeingRetaken(beingContested.CapturedBy, tank.Owner, remainingTicks);
        }
    }

    /// <summary>
    /// Handles the logic when the zone is being retaken by another player.
    /// </summary>
    private void HandleBeingRetakenState(BeingRetaken beingRetaken, List<Tank> tanksInZone)
    {
        if (tanksInZone.Count == 1)
        {
            var tank = tanksInZone.First();
            if (tank == beingRetaken.RetakenBy.Tank)
            {
                var remainingTicks = this.DecrementRemainingTicksToCapture(beingRetaken.RetakenBy);
                if (remainingTicks == 0)
                {
                    this.Status = new Captured(beingRetaken.RetakenBy);
                    this.remainingTicksToCapture.Clear();
                }
                else
                {
                    this.Status = new BeingRetaken(beingRetaken.CapturedBy, beingRetaken.RetakenBy, remainingTicks);
                }
            }
        }
        else if (tanksInZone.Count >= 2)
        {
            this.Status = new BeingContested(beingRetaken.CapturedBy);
        }
    }

    private int GetRemainingTicksToCapture(Player player)
    {
        return this.remainingTicksToCapture.GetValueOrDefault(player, TicksToCapture);
    }

    private int IncrementRemainingTicksToCapture(Player player)
    {
        return ++this.remainingTicksToCapture[player];
    }

    private int DecrementRemainingTicksToCapture(Player player)
    {
        var remainingTicks = this.remainingTicksToCapture.GetValueOrDefault(player, TicksToCapture);
        this.remainingTicksToCapture[player] = --remainingTicks;
        return remainingTicks;

    }

    private void RemovePlayerFromRemainingTicksToCapture(Player player)
    {
        _ = this.remainingTicksToCapture.Remove(player);
    }
}
