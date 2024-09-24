using System;
using GameClient.Networking;
using GameClient.Scenes.GameEndCore;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the game end scene.
/// </summary>
internal class GameEnd : Scene
{
    private readonly GameEndComponents components;
    private readonly GameEndUpdater updater;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEnd"/> class.
    /// </summary>
    public GameEnd()
        : base(Color.Transparent)
    {
        var initializer = new GameEndInitializer(this);
        this.components = new GameEndComponents(initializer);
        this.updater = new GameEndUpdater(this.components);
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        UpdateMainMenuBackgroundEffectRotation(gameTime);
        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        ScreenController.GraphicsDevice.Clear(Color.Black);
        MainMenu.Effect.Draw(gameTime);
        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += this.GameEnd_Showing;
        this.Hiding += this.GameEnd_Hiding;
    }

    private static void UpdateMainMenuBackgroundEffectRotation(GameTime gameTime)
    {
        MainMenu.Effect.Rotation += 0.3f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        MainMenu.Effect.Rotation %= MathHelper.TwoPi;
    }

    private static void Connection_ErrorThrew(string error)
    {
        DebugConsole.ThrowError(error);
        ChangeToPreviousOrDefault<MainMenu>();
    }

    private void GameEnd_Showing(object? sender, SceneDisplayEventArgs? e)
    {
        if (e is not GameEndDisplayEventArgs args)
        {
            DebugConsole.ThrowError(
                $"Game scene requires {nameof(GameEndDisplayEventArgs)}.");
            ChangeToPreviousOrDefault<MainMenu>();
            return;
        }

        this.updater.UpdateScoreboard(args.Players);

        ServerConnection.ErrorThrew += Connection_ErrorThrew;
    }

    private void GameEnd_Hiding(object? sender, EventArgs e)
    {
        ServerConnection.ErrorThrew -= Connection_ErrorThrew;
    }
}
