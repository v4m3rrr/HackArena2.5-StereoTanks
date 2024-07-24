global using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents the game client.
/// </summary>
public class MonoTanks : Game
{
#if DEBUG
    private SolidColor fpsInfo = default!;
    private SolidColor runningSlowlyInfo = default!;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoTanks"/> class.
    /// </summary>
    public MonoTanks()
    {
        Instance = this;

        var graphics = new GraphicsDeviceManager(this);
        ScreenController.Initialize(graphics, this.Window);

        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = true;
    }

    /// <summary>
    /// Gets the minimum window size for the game.
    /// </summary>
    public static Point MinWindowSize => new(640, 480);

    /// <summary>
    /// Gets the instance of the game client.
    /// </summary>
    public static MonoTanks Instance { get; private set; } = default!;

    /// <summary>
    /// Gets a value indicating whether the game is in debug mode.
    /// </summary>
#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug => false;
#endif

    /// <summary>
    /// Initializes the game.
    /// </summary>
    protected override void Initialize()
    {
        ContentController.Initialize(this.Content);

        ScreenController.Change(1366, 768, ScreenType.Windowed);
        ScreenController.ApplyChanges();

        var dc = new DebugConsole();
        dc.Initialize();
        Scene.AddScene(dc);

        Scene.InitializeScenes(typeof(Scene).Assembly);
        Scene.Change<Scenes.MainMenu>();

        base.Initialize();

#if DEBUG
        this.fpsInfo = new SolidColor(Color.Black * 0.1f)
        {
            Transform =
            {
                Type = TransformType.Absolute,
                Location = new Point(1, 1),
                Size = new Point(65, 15),
            },
        };
        _ = new Text(new ScalableFont("Content\\Fonts\\Consolas.ttf", 13), Color.White)
        {
            Parent = this.fpsInfo,
            TextShrink = TextShrinkMode.HeightAndWidth,
        };

        this.runningSlowlyInfo = new SolidColor(Color.Black * 0.1f)
        {
            Transform =
            {
                Type = TransformType.Absolute,
                Location = new Point(ScreenController.Width - 131, 1),
                Size = new Point(130, 15),
            },
        };
        ScreenController.ScreenChanged += (s, e) => this.runningSlowlyInfo.Transform.Location = new Point(ScreenController.Width - 131, 1);
        _ = new Text(new ScalableFont("Content\\Fonts\\Consolas.ttf", 13), Color.Yellow)
        {
            Parent = this.runningSlowlyInfo,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Value = "RUNNING SLOWLY",
        };
#endif
    }

    /// <summary>
    /// Loads the game content.
    /// </summary>
    protected override void LoadContent()
    {
        var spriteBatch = new SpriteBatch(this.GraphicsDevice);
        SpriteBatchController.Initialize(spriteBatch);
    }

    /// <summary>
    /// Updates the game state.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    /// <remarks>It is called once per frame.</remarks>
    protected override void Update(GameTime gameTime)
    {
        KeyboardController.Update();
        MouseController.Update();
        ScreenController.Update();

#if DEBUG
        this.runningSlowlyInfo.IsEnabled = gameTime.IsRunningSlowly;

        this.fpsInfo.Update(gameTime);
        this.runningSlowlyInfo.Update(gameTime);
#endif

        if (KeyboardController.IsKeyHit(Keys.OemTilde)
            && KeyboardController.IsKeyDown(Keys.LeftControl))
        {
            Scene.ShowOverlay<DebugConsole>(default);
        }

        if (KeyboardController.IsKeyHit(Keys.F11))
        {
            if (ScreenController.Width != 1366)
            {
                GameSettings.SetResolution(1366, 768);
                GameSettings.SetScreenType(ScreenType.Windowed);
            }
            else
            {
                GameSettings.SetResolution(1920, 1080);
                GameSettings.SetScreenType(ScreenType.Windowed);
            }
        }

        Scene.Current.Update(gameTime);
        Scene.UpdateOverlays(gameTime);

        base.Update(gameTime);
    }

    /// <summary>
    /// Draws the game.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    /// <remarks>It is called once per frame.</remarks>
    protected override void Draw(GameTime gameTime)
    {
        this.GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatchController.SpriteBatch.Begin(blendState: BlendState.NonPremultiplied);

        Scene.Current.Draw(gameTime);
        Scene.DrawOverlays(gameTime);

#if DEBUG
        this.fpsInfo.GetChild<Text>()!.Value = $"FPS: {1 / gameTime.ElapsedGameTime.TotalSeconds:0}";
        this.fpsInfo.Draw(gameTime);
        this.runningSlowlyInfo.Draw(gameTime);
#endif

        SpriteBatchController.SpriteBatch.End();

        base.Draw(gameTime);
    }
}
