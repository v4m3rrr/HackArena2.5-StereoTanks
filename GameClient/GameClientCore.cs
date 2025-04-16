using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GameClient.Networking;
using GameLogic.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents the game client.
/// </summary>
public class GameClientCore : Game
{
    /// <summary>
    /// Gets the platform the game is running on.
    /// </summary>
#if WINDOWS
    public const string Platform = "Windows";
#elif LINUX
    public const string Platform = "Linux";
#elif OSX
    public const string Platform = "macOS";
#else
#error Platform not supported.
#endif

    private static readonly ConcurrentQueue<Action> MainThreadActions = new();
    private static readonly ManualResetEventSlim ActionEvent = new(false);
    private static Thread mainThread = default!;

#if DEBUG
    private SolidColor fpsInfo = default!;
    private SolidColor runningSlowlyInfo = default!;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="GameClientCore"/> class.
    /// </summary>
    public GameClientCore()
    {
        Instance = this;
        mainThread = Thread.CurrentThread;

        var graphics = new GraphicsDeviceManager(this)
        {
            HardwareModeSwitch = false,
        };

        ScreenController.Initialize(graphics, this.Window);

        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = true;
    }

    /// <summary>
    /// Gets the theme color of the game.
    /// </summary>
#if DEBUG
    public static Color ThemeColor { get; } = (new Color(0xFF, 0x9B, 0x1A) * 0.9f).WithAlpha(255);
#else
    public static Color ThemeColor { get; } = new(0, 166, 255);
#endif

    /// <summary>
    /// Gets the start time of the application.
    /// </summary>
    public static DateTime AppStartTime { get; } = DateTime.Now;

    /// <summary>
    /// Gets the minimum window size for the game.
    /// </summary>
    public static Point MinWindowSize => new(640, 480);

    /// <summary>
    /// Gets the instance of the game client.
    /// </summary>
    public static GameClientCore Instance { get; private set; } = default!;

    /// <summary>
    /// Gets a value indicating whether the game is in debug mode.
    /// </summary>
#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug => false;
#endif

    /// <summary>
    /// Invokes a function on the main thread.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="func">The function to invoke.</param>
    /// <returns>The result of the function.</returns>
    /// <remarks>
    /// If the function is called on the main thread,
    /// it is executed immediately.
    /// Otherwise, it is enqueued and executed on
    /// the main thread at the end of the update loop.
    /// </remarks>
    public static T InvokeOnMainThread<T>(Func<T> func)
    {
        if (Thread.CurrentThread == mainThread)
        {
            return func();
        }

        T result = default!;
        var resetEvent = new ManualResetEventSlim(false);

        var action = new Action(() =>
        {
            result = func();
            resetEvent.Set();
        });

        MainThreadActions.Enqueue(action);
        ActionEvent.Set();

        resetEvent.Wait();

        return result;
    }

    /// <summary>
    /// Invokes an action on the main thread.
    /// </summary>
    /// <param name="action">The action to invoke.</param>
    /// <remarks>
    /// If the action is called on the main thread,
    /// it is executed immediately.
    /// Otherwise, it is enqueued and executed on
    /// the main thread at the end of the update loop.
    /// </remarks>
    public static void InvokeOnMainThread(Action action)
    {
        if (Thread.CurrentThread == mainThread)
        {
            action();
            return;
        }

        MainThreadActions.Enqueue(action);
        ActionEvent.Set();
    }

    /// <summary>
    /// Invokes a function on the main thread asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="func">The function to invoke.</param>
    /// <returns>The result of the function.</returns>
    /// <remarks>
    /// If the function is called on the main thread,
    /// it is executed immediately.
    /// Otherwise, it is enqueued and executed on
    /// the main thread at the end of the update loop.
    /// </remarks>
    public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
    {
        if (Thread.CurrentThread == mainThread)
        {
            return Task.FromResult(func());
        }

        var tcs = new TaskCompletionSource<T>();

        var action = new Action(() =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        MainThreadActions.Enqueue(action);
        ActionEvent.Set();

        return tcs.Task;
    }

    /// <summary>
    /// Invokes an action on the main thread asynchronously.
    /// </summary>
    /// <param name="action">The action to invoke.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// If the action is called on the main thread,
    /// it is executed immediately.
    /// Otherwise, it is enqueued and executed on
    /// the main thread at the end of the update loop.
    /// </remarks>
    public static Task InvokeOnMainThreadAsync(Action action)
    {
        if (Thread.CurrentThread == mainThread)
        {
            action();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>();

        MainThreadActions.Enqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        ActionEvent.Set();

        return tcs.Task;
    }

    /// <summary>
    /// Initializes the game.
    /// </summary>
    protected override void Initialize()
    {
        this.Window.Title = nameof(GameClientCore);

        ContentController.Initialize(this.Content);

        ScreenController.Change(1366, 768, ScreenType.Windowed);

        ScreenController.ScreenChanged += (s, e) =>
        {
            if (ScreenController.ScreenType is ScreenType.FullScreen)
            {
                var matrix = Matrix.CreateScale(
                1f / ScreenController.ViewportScale.X,
                1f / ScreenController.ViewportScale.Y,
                1.0f);

                ScreenController.TransformMatrix = matrix;
            }
            else
            {
                ScreenController.TransformMatrix = null;
            }
        };
        ScreenController.ApplyChanges();

        var spriteBatch = new SpriteBatch(this.GraphicsDevice);
        SpriteBatchController.Initialize(spriteBatch);

        var dc = new DebugConsole();
        dc.Initialize();
        dc.LoadContent();
        Scene.AddScene(dc);

        var loading = new Scenes.Loading();
        loading.Initialize();
        loading.LoadContent();
        Scene.AddScene(loading);
        Scene.Change<Scenes.Loading>();

        base.Initialize();

        PacketSerializer.ExceptionThrew += (e) => DebugConsole.ThrowError(e);
        Packet.GetPayloadFailed += (e) => DebugConsole.ThrowError(e);

        ServerConnection.MessageReceived += (s, e) =>
        {
            var packet = PacketSerializer.Deserialize(e);
            if (packet.Type.HasFlag(PacketType.ErrorGroup))
            {
                DebugConsole.ThrowError("Server error: " + packet.GetPayload<ErrorPayload>().Message);
            }
        };

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

        this.fpsInfo.Load();

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

        this.runningSlowlyInfo.Load();

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
        _ = Task.Run(async () =>
        {
            try
            {
                Localization.Initialize();

                await GameSettings.LoadSettings();
                await Scenes.JoinRoomCore.JoinData.Load();

                Scene.InitializeScenes(typeof(Scene).Assembly);
                Scene.LoadAllContent();
                Scene.ChangeWithoutStack<Scenes.MainMenu>();
            }
            catch (Exception ex)
            {
                DebugConsole.ThrowError(ex);
                DebugConsole.Open();
            }
        });
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

#if DEBUG
        this.runningSlowlyInfo.IsEnabled = gameTime.IsRunningSlowly;

        this.fpsInfo.Update(gameTime);
        this.runningSlowlyInfo.Update(gameTime);
#endif

        if (KeyboardController.IsKeyHit(Keys.OemTilde)
            && KeyboardController.IsKeyDown(Keys.LeftControl))
        {
            DebugConsole.Open();
        }

        Scene.Current.Update(gameTime);
        ScreenController.UpdateOverlays(gameTime);

        base.Update(gameTime);

        while (MainThreadActions.TryDequeue(out var action))
        {
            action();
        }
    }

    /// <summary>
    /// Draws the game.
    /// </summary>
    /// <param name="gameTime">The game time.</param>
    /// <remarks>It is called once per frame.</remarks>
    protected override void Draw(GameTime gameTime)
    {
        this.GraphicsDevice.Clear(Color.Black);

        SpriteBatchController.SpriteBatch.Begin(
            blendState: BlendState.NonPremultiplied,
            transformMatrix: ScreenController.TransformMatrix);

        Scene.Current.Draw(gameTime);
        ScreenController.DrawOverlays(gameTime);

#if DEBUG
        this.fpsInfo.GetChild<Text>()!.Value = $"FPS: {1 / gameTime.ElapsedGameTime.TotalSeconds:0}";
        this.fpsInfo.Draw(gameTime);
        this.runningSlowlyInfo.Draw(gameTime);
#endif

        SpriteBatchController.SpriteBatch.End();

        base.Draw(gameTime);
    }
}
