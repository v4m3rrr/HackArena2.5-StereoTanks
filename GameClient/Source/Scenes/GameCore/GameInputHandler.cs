using System;
using System.Collections.Generic;
using GameLogic;
using GameLogic.Networking;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient.Scenes.GameCore;

/// <summary>
/// Represents the keyboard handler.
/// </summary>
internal class GameInputHandler
{
    private static readonly List<Func<IPacketPayload?>> Handlers = [
        () =>
        {
            TankRotationPayload? payload = HandleTankRotationPayload();
            payload = HandleTurretRotationPayload(payload);
            return payload;
        },
        HandleTankMovementPayload,
        HandleTankShootPayload,
    ];

    /// <summary>
    /// Handles the input payload.
    /// </summary>
    /// <returns>
    /// The handled input payload.
    /// </returns>
    public static IPacketPayload? HandleInputPayload()
    {
        IPacketPayload? payload = null;

        foreach (var handler in Handlers)
        {
            payload = handler();
            if (payload != null)
            {
                return payload;
            }
        }

#if DEBUG
        if (KeyboardController.IsKeyHit(Keys.T))
        {
            payload = new EmptyPayload() { Type = PacketType.ShootAll };
        }
#endif
        return payload;
    }

    private static TankRotationPayload? HandleTankRotationPayload()
    {
        TankRotationPayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.A))
        {
            return new TankRotationPayload() { TankRotation = Rotation.Left };
        }
        else if (KeyboardController.IsKeyHit(Keys.D))
        {
            return new TankRotationPayload() { TankRotation = Rotation.Right };
        }

        return payload;
    }

    private static TankRotationPayload? HandleTurretRotationPayload(TankRotationPayload? payload)
    {
        if (KeyboardController.IsKeyHit(Keys.Q))
        {
            payload = new TankRotationPayload() { TankRotation = payload?.TankRotation, TurretRotation = Rotation.Left };
        }
        else if (KeyboardController.IsKeyHit(Keys.E))
        {
            payload = new TankRotationPayload() { TankRotation = payload?.TankRotation, TurretRotation = Rotation.Right };
        }

        return payload;
    }

    private static TankMovementPayload? HandleTankMovementPayload()
    {
        TankMovementPayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.W))
        {
            payload = new TankMovementPayload(TankMovement.Forward);
        }
        else if (KeyboardController.IsKeyHit(Keys.S))
        {
            payload = new TankMovementPayload(TankMovement.Backward);
        }

        return payload;
    }

    private static TankShootPayload? HandleTankShootPayload()
    {
        TankShootPayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.Space))
        {
            payload = new TankShootPayload();
        }

        return payload;
    }
}
