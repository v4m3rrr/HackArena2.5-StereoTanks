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
#if STEREO

    private readonly TankType? tankType;

#pragma warning disable IDE0290
    /// <summary>
    /// Initializes a new instance of the <see cref="GameInputHandler"/> class.
    /// </summary>
    /// <param name="tankType">The type of the tank.</param>
    public GameInputHandler(TankType? tankType)
    {
        this.tankType = tankType;
    }
#pragma warning restore IDE0290

#endif

    /// <summary>
    /// Handles the input.
    /// </summary>
    /// <returns>
    /// The handled input payload.
    /// </returns>
    public IPacketPayload? HandleInput()
    {
        IPacketPayload? payload = null;

        foreach (var handler in this.GetHandlers())
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
#if !STEREO
                GiveSecondaryItemPayload g => new GlobalGiveSecondaryItemPayload(g.Item),
#endif
                _ => payload,
            };
        }

#endif

        return payload;
    }

    /// <summary>
    /// Gets the handlers for the input payload.
    /// </summary>
    /// <returns>The handlers for the input payload.</returns>
    protected internal IEnumerable<Func<IPacketPayload?>> GetHandlers()
    {
        yield return () =>
        {
            RotationPayload? payload = HandleTankRotationPayload();
            payload = HandleTurretRotationPayload(payload);
            return payload;
        };

        yield return HandleMovementPayload;

#if DEBUG && !STEREO
        yield return HandleGiveSecondaryItemPayload;
#endif

#if DEBUG && STEREO
        yield return this.HandleFullyRegenerateAbilityPayload;
#endif

#if STEREO
        if (this.tankType is { } tankType)
        {
            yield return () => HandleAbilityUsePayload(tankType);
        }

        yield return () =>
        {
            GoToPayload? payload = HandleGoToPayload();
            RotationPayload? rotationPayload = HandleTurretRotationPayload(null);
            if (payload is not null && rotationPayload is not null)
            {
                payload = new GoToPayload(payload.X, payload.Y)
                {
                    TurretRotation = rotationPayload.TurretRotation,
                };
            }

            return payload;
        };
#else
        yield return HandleAbilityUsePayload;
#endif
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

#if STEREO

    private static GoToPayload? HandleGoToPayload()
    {
        if (!MouseController.IsLeftButtonPressed())
        {
            return null;
        }

        if (Scene.Current is not Game gameScene)
        {
            return null;
        }

        var gridComponent = gameScene.BaseComponent.GetChild<GridComponent>();
        if (gridComponent is null || !MouseController.IsComponentFocused(gridComponent))
        {
            return null;
        }

        var mousePosition = MouseController.Position;
        var gridLocation = gridComponent.Transform.Location;
        var tileSize = gridComponent.TileSize;

        if (tileSize == 0)
        {
            return null;
        }

        var tileX = (mousePosition.X - gridLocation.X) / tileSize;
        var tileY = (mousePosition.Y - gridLocation.Y) / tileSize;
        return new GoToPayload(tileX, tileY);
    }

#endif

#if STEREO
    private static AbilityUsePayload? HandleAbilityUsePayload(TankType tankType)
#else
    private static AbilityUsePayload? HandleAbilityUsePayload()
#endif
    {
        AbilityUsePayload? payload = null;

        if (KeyboardController.IsKeyHit(Keys.Space))
        {
            payload = new AbilityUsePayload(AbilityType.FireBullet);
        }
#if STEREO
        else if (tankType is TankType.Heavy && KeyboardController.IsKeyHit(Keys.D1))
#else
        else if (KeyboardController.IsKeyHit(Keys.D1))
#endif
        {
            payload = new AbilityUsePayload(AbilityType.UseLaser);
        }
#if STEREO
        else if (tankType is TankType.Light && KeyboardController.IsKeyHit(Keys.D1))
#else
        else if (KeyboardController.IsKeyHit(Keys.D2))
#endif
        {
            payload = new AbilityUsePayload(AbilityType.FireDoubleBullet);
        }
#if STEREO
        else if (tankType is TankType.Light && KeyboardController.IsKeyHit(Keys.D2))
#else
        else if (KeyboardController.IsKeyHit(Keys.D3))
#endif
        {
            payload = new AbilityUsePayload(AbilityType.UseRadar);
        }
#if STEREO
        else if (tankType is TankType.Heavy && KeyboardController.IsKeyHit(Keys.D2))
#else
        else if (KeyboardController.IsKeyHit(Keys.D4))
#endif
        {
            payload = new AbilityUsePayload(AbilityType.DropMine);
        }
#if STEREO
        else if (KeyboardController.IsKeyHit(Keys.D3))
        {
            payload = new AbilityUsePayload(AbilityType.FireHealingBullet);
        }
        else if (KeyboardController.IsKeyHit(Keys.D4))
        {
            payload = new AbilityUsePayload(AbilityType.FireStunBullet);
        }
#endif

        return payload;
    }

#if DEBUG && STEREO

    private FullyRegenerateAbilityPayload? HandleFullyRegenerateAbilityPayload()
    {
        if (!KeyboardController.IsKeyDown(Keys.LeftControl))
        {
            return null;
        }

        FullyRegenerateAbilityPayload? payload = null;

        AbilityType? ability;
        if (KeyboardController.IsKeyHit(Keys.D1))
        {
            ability = this.tankType switch
            {
                TankType.Light => AbilityType.FireDoubleBullet,
                TankType.Heavy => AbilityType.UseLaser,
                _ => null,
            };
        }
        else if (KeyboardController.IsKeyHit(Keys.D2))
        {
            ability = this.tankType switch
            {
                TankType.Light => AbilityType.UseRadar,
                TankType.Heavy => AbilityType.DropMine,
                _ => null,
            };
        }
        else if (KeyboardController.IsKeyHit(Keys.D3))
        {
            ability = AbilityType.FireHealingBullet;
        }
        else if (KeyboardController.IsKeyHit(Keys.D4))
        {
            ability = AbilityType.FireStunBullet;
        }
        else
        {
            return null;
        }

        return new FullyRegenerateAbilityPayload(ability!.Value);
    }

#endif

#if DEBUG && !STEREO

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
