using System.Text;
using GameLogic;
using GameLogic.Networking;
using GameServer.Enums;
using GameServer.Services;
using Serilog;

namespace GameServer;

#if STEREO

/// <summary>
/// Handles GoTo requests from players.
/// </summary>
internal sealed class GoToService(GameInstance game, ILogger logger)
{
    private readonly PlayerActionHandler actionHandler = new(game, logger);

    /// <summary>
    /// Tries to validate and resolve the GoTo context from the given packet.
    /// </summary>
    /// <param name="player">The player connection.</param>
    /// <param name="packet">The received packet.</param>
    /// <param name="context">The resolved GoTo context if valid.</param>
    /// <param name="responsePayload">The optional response payload to send back to the player.</param>
    /// <returns><see langword="true"/> if resolution succeeded; otherwise, <see langword="false"/>.</returns>
    public bool TryResolve(
        PlayerConnection player,
        Packet packet,
        out Context? context,
        out IPacketPayload? responsePayload)
    {
        context = null;
        responsePayload = null;

        var payload = packet.GetPayload<GoToPayload>(out var exception);
        if (exception is not null)
        {
            responsePayload = new ErrorPayload(PacketType.InternalErrorWithPayload, exception.Message);
            return false;
        }

        if (player.LastGameStatePayload is null)
        {
            responsePayload = new ErrorPayload(
                PacketType.InternalErrorWithPayload,
                "Missing sent game state payload to resolve GoTo.");

            return false;
        }

        if (!game.Grid.IsCellWithinBounds(payload.X, payload.Y))
        {
            responsePayload = new ErrorPayload(
                PacketType.InvalidPacketUsageErrorWithPayload,
                "GoTo coordinates are out of bounds.");

            return false;
        }

        if (game.Grid.WallGrid[payload.X, payload.Y] is not null)
        {
            responsePayload = new ErrorPayload(
                PacketType.InvalidPacketUsageErrorWithPayload,
                "GoTo coordinates lead to a wall.");

            return false;
        }

        var pathFinder = new PathFinder(game.Settings, player.LastGameStatePayload, player.Instance);
        context = new Context(player, payload!, pathFinder);
        return true;
    }

    /// <summary>
    /// Executes the GoTo logic based on a previously resolved context.
    /// </summary>
    /// <param name="context">The GoTo context to execute.</param>
    public void Execute(Context context)
    {
        var action = context.PathFinder.GetNextAction(
            context.Payload.X,
            context.Payload.Y,
            context.Payload.Costs,
            context.Payload.Penalties);

        if (action is null)
        {
            return;
        }

        switch (action)
        {
            case PathAction.MoveForward:
                this.actionHandler.HandleMovement(context.Player, MovementDirection.Forward);
                break;

            case PathAction.MoveBackward:
                this.actionHandler.HandleMovement(context.Player, MovementDirection.Backward);
                break;

            case PathAction.RotateLeft:
                this.actionHandler.HandleRotation(context.Player, Rotation.Left, context.Payload.TurretRotation);
                break;

            case PathAction.RotateRight:
                this.actionHandler.HandleRotation(context.Player, Rotation.Right, context.Payload.TurretRotation);
                break;
        }
    }

    /// <summary>
    /// Represents the resolved GoTo context.
    /// </summary>
    /// <param name="Player">The player connection.</param>
    /// <param name="Payload">The GoTo payload.</param>
    /// <param name="PathFinder">The pathfinding context.</param>
    internal sealed record Context(
        PlayerConnection Player,
        GoToPayload Payload,
        PathFinder PathFinder);
}

#endif
