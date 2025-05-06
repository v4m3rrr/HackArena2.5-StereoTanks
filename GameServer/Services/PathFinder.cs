using System.Collections.Concurrent;
using GameLogic;
using GameLogic.Networking;
using GameLogic.Networking.GoToElements;
using GameServer.Enums;

namespace GameServer.Services;

#if STEREO && HACKATHON

/// <summary>
/// Represents a pathfinding algorithm.
/// </summary>
internal class PathFinder
{
    private readonly GameStatePayload.ForPlayer gameState;
    private readonly Grid grid;
    private readonly int gridDim;
    private readonly bool[,] wallGrid;
    private readonly Tank tank;

    /// <summary>
    /// Initializes a new instance of the <see cref="PathFinder"/> class.
    /// </summary>
    /// <param name="serverSettings">The server settings.</param>
    /// <param name="gameState">
    /// The game state payload for the player for whom the pathfinding is performed.
    /// </param>
    /// <param name="player">The player for whom the pathfinding is performed.</param>
    public PathFinder(ServerSettings serverSettings, GameStatePayload.ForPlayer gameState, Player player)
    {
        this.gameState = gameState;
        this.gridDim = serverSettings.GridDimension;
        this.tank = player.Tank;

        this.grid = new Grid(this.gridDim, serverSettings.Seed);
        this.grid.UpdateFromGameStatePayload(gameState);

        this.wallGrid = new bool[this.gridDim, this.gridDim];
        for (int i = 0; i < this.gridDim; i++)
        {
            for (int j = 0; j < this.gridDim; j++)
            {
                this.wallGrid[i, j] = this.grid.WallGrid[i, j] is not null;
            }
        }
    }

    /// <summary>
    /// Finds the next action to take in order to reach the target position.
    /// </summary>
    /// <param name="targetX">The X coordinate of the target position.</param>
    /// <param name="targetY">The Y coordinate of the target position.</param>
    /// <param name="costs">The costs associated with each action.</param>
    /// <param name="penalties">The penalties associated with each action.</param>
    /// <returns>
    /// The next action to take in order to reach the target position,
    /// or <see langword="null"/> if no action is nedeed or possible.
    /// </returns>
    public PathAction? GetNextAction(int targetX, int targetY, Costs costs, Penalties penalties)
    {
        var visited = new HashSet<(int X, int Y, Direction Direction)>();
        var queue = new PriorityQueue<Node, float>();

        var start = new Node(
            this.tank.X,
            this.tank.Y,
            this.tank.Direction,
            null,
            0f,
            null,
            this.gameState.Tick);

        queue.Enqueue(start, 0);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.X == targetX && current.Y == targetY)
            {
                return ReconstructFirstAction(current);
            }

            if (!visited.Add((current.X, current.Y, current.Direction)))
            {
                continue;
            }

            this.SimulateBullets();

            float currentPenalty = this.GetDangerPenalty(current.X, current.Y, penalties, current.Tick);
            var nextTick = current.Tick + 1;

            if (this.TryMove(current.X, current.Y, current.Direction, out int fx, out int fy))
            {
                float penalty = this.GetDangerPenalty(fx, fy, penalties, nextTick);
                var forwardNode = new Node(
                    fx,
                    fy,
                    current.Direction,
                    current,
                    current.Cost + costs.Forward + penalty,
                    PathAction.MoveForward,
                    nextTick);
                queue.Enqueue(forwardNode, forwardNode.Cost);
            }

            if (this.TryMoveBackward(current.X, current.Y, current.Direction, out int bx, out int by))
            {
                float penalty = this.GetDangerPenalty(bx, by, penalties, nextTick);
                var backwardNode = new Node(
                    bx,
                    by,
                    current.Direction,
                    current,
                    current.Cost + costs.Backward + penalty,
                    PathAction.MoveBackward,
                    nextTick);
                queue.Enqueue(backwardNode, backwardNode.Cost);
            }

            var leftDirection = EnumUtils.Previous(current.Direction);
            var leftNode = new Node(
                current.X,
                current.Y,
                leftDirection,
                current,
                current.Cost + costs.Rotate + currentPenalty,
                PathAction.RotateLeft,
                nextTick);
            queue.Enqueue(leftNode, leftNode.Cost);

            var rightDirection = EnumUtils.Next(current.Direction);
            var rightNode = new Node(
                current.X,
                current.Y,
                rightDirection,
                current,
                current.Cost + costs.Rotate + currentPenalty,
                PathAction.RotateRight,
                nextTick);
            queue.Enqueue(rightNode, rightNode.Cost);
        }

        return null;
    }

    private static PathAction? ReconstructFirstAction(Node node)
    {
        while (node.Previous != null && node.Previous.Previous != null)
        {
            node = node.Previous;
        }

        return node.Action;
    }

    private bool TryMove(int x, int y, Direction direction, out int newX, out int newY)
    {
        var (nx, ny) = DirectionUtils.Normal(direction);
        (newX, newY) = (x + nx, y + ny);
        return this.IsInBounds(newX, newY) && !this.wallGrid[newX, newY];
    }

    private bool TryMoveBackward(int x, int y, Direction direction, out int newX, out int newY)
    {
        var (nx, ny) = DirectionUtils.Normal(direction);
        (newX, newY) = (x - nx, y - ny);
        return this.IsInBounds(newX, newY) && !this.wallGrid[newX, newY];
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < this.gridDim && y < this.gridDim;
    }

    private void SimulateBullets()
    {
        foreach (var bullet in this.grid.Bullets)
        {
            bullet.UpdatePosition(deltaTime: 1f);
        }
    }

    private float GetDangerPenalty(int x, int y, Penalties penalties, int tick)
    {
        var penalty = 0f;

        penalty += penalties.Bullet * this.grid.Bullets.Count(b => b.X == x && b.Y == y);
        penalty += penalties.Mine * this.grid.Mines.Count(b => b.X == x && b.Y == y);
        penalty += penalties.Laser * this.grid.Lasers.Count(b => b.X == x && b.Y == y);

        if (tick == this.gameState.Tick + 1 && !this.gameState.VisibilityGrid[x, y])
        {
            penalty += penalties.Blindly;
        }

        penalty += penalties.PerTile
            .Where(p => p.X == x && p.Y == y)
            .Select(p => p.Penalty)
            .Sum();

        return penalty;
    }

    private record class Node(
        int X,
        int Y,
        Direction Direction,
        Node? Previous,
        float Cost,
        PathAction? Action,
        int Tick);
}

#endif
