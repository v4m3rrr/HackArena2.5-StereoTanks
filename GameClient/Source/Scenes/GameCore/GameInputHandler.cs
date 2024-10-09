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
            RotationPayload? payload = HandleTankRotationPayload();
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

    private static RotationPayload? HandleTankRotationPayload()
    {
        RotationPayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.A))
        {
            return new RotationPayload() { TankRotation = Rotation.Left };
        }
        else if (KeyboardController.IsKeyHit(Keys.D))
        {
            return new RotationPayload() { TankRotation = Rotation.Right };
        }

        return payload;
    }

    private static RotationPayload? HandleTurretRotationPayload(RotationPayload? payload)
    {
        if (KeyboardController.IsKeyHit(Keys.Q))
        {
            payload = new RotationPayload() { TankRotation = payload?.TankRotation, TurretRotation = Rotation.Left };
        }
        else if (KeyboardController.IsKeyHit(Keys.E))
        {
            payload = new RotationPayload() { TankRotation = payload?.TankRotation, TurretRotation = Rotation.Right };
        }

        return payload;
    }

    private static MovementPayload? HandleTankMovementPayload()
    {
        MovementPayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.W))
        {
            payload = new MovementPayload(MovementDirection.Forward);
        }
        else if (KeyboardController.IsKeyHit(Keys.S))
        {
            payload = new MovementPayload(MovementDirection.Backward);
        }

        return payload;
    }

    private static AbilityUsePayload? HandleTankShootPayload()
    {
        AbilityUsePayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.Space))
        {
            payload = new AbilityUsePayload(AbilityType.FireBullet);
        }

        return payload;
    }
}
