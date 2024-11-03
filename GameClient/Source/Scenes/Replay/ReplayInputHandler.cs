using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient.Scenes.Replay;

/// <summary>
/// Represents the replay input handler.
/// </summary>
internal class ReplayInputHandler
{
    private DateTime? leftKeyDownTime;
    private DateTime? rightKeyDownTime;

    /// <summary>
    /// Updates the input handler.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    /// <param name="isRunning">The value indicating whether the replay is running.</param>
    /// <param name="current">The current time span of the replay.</param>
    public void Update(GameTime gameTime, ref bool isRunning, ref TimeSpan current)
    {
        isRunning ^= KeyboardController.IsKeyHit(Keys.Space);

        bool isLeftKeyHit = KeyboardController.IsKeyHit(Keys.Left);
        bool isRightKeyHit = KeyboardController.IsKeyHit(Keys.Right);
        int factor = KeyboardController.IsKeyDown(Keys.LeftControl)
            ? 1000 / Game.ServerBroadcastInterval * 60
            : 1;

        if (isLeftKeyHit)
        {
            current -= factor * TimeSpan.FromMilliseconds(Game.ServerBroadcastInterval);
        }

        if (isRightKeyHit)
        {
            current += factor * TimeSpan.FromMilliseconds(Game.ServerBroadcastInterval);
        }

        this.leftKeyDownTime = KeyboardController.IsKeyDown(Keys.Left)
            ? (this.leftKeyDownTime ?? DateTime.Now)
            : null;

        this.rightKeyDownTime = KeyboardController.IsKeyDown(Keys.Right)
            ? (this.rightKeyDownTime ?? DateTime.Now)
            : null;

        if (this.leftKeyDownTime is { } lkdt)
        {
            if ((DateTime.Now - lkdt) > TimeSpan.FromSeconds(0.5))
            {
                current -= 2.5f * TimeSpan.FromMilliseconds(Game.ServerBroadcastInterval);
            }

            if (isRunning)
            {
                current -= gameTime.ElapsedGameTime;
            }

            current = current < TimeSpan.Zero ? TimeSpan.Zero : current;
        }

        if (this.rightKeyDownTime is { } rkdt)
        {
            if ((DateTime.Now - rkdt) > TimeSpan.FromSeconds(0.5))
            {
                current += 2.5f * TimeSpan.FromMilliseconds(Game.ServerBroadcastInterval);
            }
        }

        if (isRunning)
        {
            current += gameTime.ElapsedGameTime;
        }
    }

    /// <summary>
    /// Resets the input handler.
    /// </summary>
    public void Reset()
    {
        this.leftKeyDownTime = null;
        this.rightKeyDownTime = null;
    }
}
