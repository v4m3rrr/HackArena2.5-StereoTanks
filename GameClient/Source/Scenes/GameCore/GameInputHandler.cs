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
internal static class GameInputHandler
{
    private static readonly List<Func<IPacketPayload?>> Handlers = [
        () =>
        {
            RotationPayload? payload = HandleTankRotationPayload();
            payload = HandleTurretRotationPayload(payload);
            return payload;
        },
        HandleMovementPayload,
#if DEBUG
        HandleGiveSecondaryItemPayload,
#endif
        HandleAbilityUsePayload,
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
                break;
            }
        }

#if DEBUG

        if (KeyboardController.IsKeyDown(Keys.LeftShift))
        {
            payload = payload switch
            {
                AbilityUsePayload a => new GlobalAbilityUsePayload(a.AbilityType),
                GiveSecondaryItemPayload g => new GlobalGiveSecondaryItemPayload(g.Item),
                _ => payload,
            };
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

    private static MovementPayload? HandleMovementPayload()
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

    private static AbilityUsePayload? HandleAbilityUsePayload()
    {
        AbilityUsePayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.Space))
        {
            payload = new AbilityUsePayload(AbilityType.FireBullet);
        }
        else if (KeyboardController.IsKeyHit(Keys.D1))
        {
            payload = new AbilityUsePayload(AbilityType.UseLaser);
        }
        else if (KeyboardController.IsKeyHit(Keys.D2))
        {
            payload = new AbilityUsePayload(AbilityType.FireDoubleBullet);
        }
        else if (KeyboardController.IsKeyHit(Keys.D3))
        {
            payload = new AbilityUsePayload(AbilityType.UseRadar);
        }
        else if (KeyboardController.IsKeyHit(Keys.D4))
        {
            payload = new AbilityUsePayload(AbilityType.DropMine);
        }

        return payload;
    }

#if DEBUG

    private static GiveSecondaryItemPayload? HandleGiveSecondaryItemPayload()
    {
        if (!KeyboardController.IsKeyDown(Keys.LeftControl))
        {
            return null;
        }

        GiveSecondaryItemPayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.D1))
        {
            payload = new GiveSecondaryItemPayload(SecondaryItemType.Laser);
        }
        else if (KeyboardController.IsKeyHit(Keys.D2))
        {
            payload = new GiveSecondaryItemPayload(SecondaryItemType.DoubleBullet);
        }
        else if (KeyboardController.IsKeyHit(Keys.D3))
        {
            payload = new GiveSecondaryItemPayload(SecondaryItemType.Radar);
        }
        else if (KeyboardController.IsKeyHit(Keys.D4))
        {
            payload = new GiveSecondaryItemPayload(SecondaryItemType.Mine);
        }

        return payload;
    }

#endif
}
