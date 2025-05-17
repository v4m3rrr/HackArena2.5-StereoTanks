using System.Net.WebSockets;
using System.Threading.Tasks;
using GameClient.Networking;
using GameClient.Scenes.GameCore;
using GameClient.Scenes.GameOverlays;
using GameClient.Scenes.Replay;
using GameLogic;
using GameLogic.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the game scene.
/// </summary>
[AutoInitialize]
internal class Game : Scene
{
#if STEREO
    private readonly List<Team> teams = [];
#else
    private readonly Dictionary<string, Player> players = [];
#endif
    private readonly GameComponents components;
    private readonly GameUpdater updater;
    private readonly ReplayInputHandler replayInputHandler = new();

    private bool isContentLoading;
    private bool isContentUpdatedAfterLoad;

    private ReplaySceneDisplayEventArgs? replayArgs;
    private TimeSpan replayTime = TimeSpan.Zero;
    private int replayTick;
    private bool isReplayLoaded;
    private bool isReplayRunning;
    private bool isReplay;

    private GameStatePayload[] replayGameStates = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game()
        : base(Color.Transparent)
    {
        var initializer = new GameInitializer(this);
        this.components = new GameComponents(initializer);
#if STEREO
        this.updater = new GameUpdater(this.components, this.teams);
#else
        this.updater = new GameUpdater(this.components, this.players);
#endif
    }

    /// <summary>
    /// Gets or sets the server settings.
    /// </summary>
    public static ServerSettings? Settings { get; set; }

    /// <summary>
    /// Gets the server broadcast interval in milliseconds.
    /// </summary>
    /// <value>
    /// The server broadcast interval in seconds.
    /// When the value is -1, the server broadcast interval is not received yet.
    /// </value>
    public static int ServerBroadcastInterval => Settings?.BroadcastInterval ?? -1;

    /// <summary>
    /// Gets or sets the player ID.
    /// </summary>
    /// <remarks>
    /// If client is a spectator, this property is <see langword="null"/>.
    /// </remarks>
    public static string? PlayerId { get; set; }

#if STEREO

    private IEnumerable<Player> Players => this.teams.SelectMany(t => t.Players);

    private Player? Player => this.Players.FirstOrDefault(p => p.Id == PlayerId);

#endif

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        UpdateMainMenuBackgroundEffectRotation(gameTime);

        if (this.isContentUpdatedAfterLoad && KeyboardController.IsKeyHit(Keys.Escape))
        {
            if (ScreenController.DisplayedOverlays.Any(x => x.Value is GameMenu or Scenes.Settings))
            {
                HideOverlay<GameMenu>();
                HideOverlay<Settings>();
            }
            else if (ScreenController.DisplayedOverlays.Any(x => x.Value is GameQuitConfirm))
            {
                HideOverlay<GameQuitConfirm>();
                HideOverlay<GameMenu>();
            }
            else
            {
                ShowOverlay<GameMenu>();
            }
        }

        if (!ScreenController.DisplayedOverlays.Any())
        {
#if STEREO
            this.HandleInput();
#else
            HandleInput();
#endif
        }

        if (this.IsContentLoaded)
        {
            this.isContentUpdatedAfterLoad = true;
        }

        if (this.isReplay && this.isReplayLoaded)
        {
            this.UpdateReplay(gameTime);
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        ScreenController.GraphicsDevice.Clear(Color.Black);
        MainEffect.Draw();

        if (this.isContentLoading || !this.isContentUpdatedAfterLoad)
        {
            return;
        }

        if (this.isReplay && !this.isReplayLoaded)
        {
            return;
        }

        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += this.Game_Showing;
        this.Hiding += this.Game_Hiding;
        this.Hid += this.Game_Hid;
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        var textures = this.BaseComponent.GetAllDescendants<TextureComponent>();
        textures.ToList().ForEach(x => x.Load());

        GameSceneComponents.PlayerBarComponent.LoadContent();

        Sprites.Bullet.LoadContent();
        Sprites.FogOfWar.LoadContent();
        Sprites.Mine.LoadContent();
#if !STEREO
        Sprites.SecondaryItem.LoadContent();
#endif
        Sprites.Tank.LoadContent();
        Sprites.Wall.LoadContent();
        Sprites.Zone.LoadContent();
    }

    private static void UpdateMainMenuBackgroundEffectRotation(GameTime gameTime)
    {
        if (MainEffect.Rotation != 0.0f)
        {
            int sign = MainEffect.Rotation is > MathHelper.Pi or < 0 and > -MathHelper.Pi ? 1 : -1;
            var value = 0.25f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MainEffect.Rotation += Math.Min(Math.Abs(MainEffect.Rotation), value) * sign;
            MainEffect.Rotation %= MathHelper.TwoPi;
        }
    }

#if STEREO
    private async void HandleInput()
#else
    private static async void HandleInput()
#endif
    {
#if STEREO
        var inputHandler = new GameInputHandler(this.Player?.Tank?.Type);
#else
        var inputHandler = new GameInputHandler();
#endif

        var payload = inputHandler.HandleInput();

        if (ServerConnection.Data is { } serverData && serverData.IsSpectator && payload is ActionPayload)
        {
            return;
        }

        if (payload is not null)
        {
            var message = PacketSerializer.Serialize(payload);
            await ServerConnection.SendAsync(message);
        }
    }

    private void Connection_MessageReceived(WebSocketReceiveResult result, string message)
    {
        if (result.MessageType == WebSocketMessageType.Close)
        {
            GameServerMessageHandler.HandleCloseMessage(result);
            return;
        }

        if (result.MessageType == WebSocketMessageType.Text)
        {
            Packet packet = PacketSerializer.Deserialize(message);
            switch (packet.Type)
            {
                case PacketType.Ping:
                    GameServerMessageHandler.HandlePingPacket();
                    break;

                case PacketType.GameState:
                    GameServerMessageHandler.HandleGameStatePacket(packet, this.updater);
                    break;

                case PacketType.GameEnded:
                    GameServerMessageHandler.HandleGameEndPacket(packet);
                    break;

                case PacketType.LobbyData:
                    GameServerMessageHandler.HandleLobbyDataPacket(packet, this.updater);
                    break;

                case PacketType.GameStarted:
                    break;

                case PacketType.PlayerAlreadyMadeActionWarning:
                    break;

                case PacketType.CustomWarning:
                    var customWarning = packet.GetPayload<CustomWarningPayload>();
                    DebugConsole.SendMessage($"Server warning: {customWarning.Message}", Color.Yellow);
                    break;

                default:
                    if (!packet.Type.IsGroup(PacketType.ErrorGroup))
                    {
                        DebugConsole.SendMessage(
                            $"Unknown packet type in Game scene: {packet.Type}",
                            Color.Yellow);
                    }

                    break;
            }
        }
    }

    private void UpdateReplay(GameTime gameTime)
    {
        this.replayInputHandler.Update(gameTime, ref this.isReplayRunning, ref this.replayTime);

        var lastReplayIndex = this.replayTick;
        this.replayTick = Math.Max((int)(this.replayTime.TotalMilliseconds / Settings!.BroadcastInterval), 0);

        if (this.replayTick >= this.replayGameStates.Length)
        {
#if STEREO
            var args = new GameEndDisplayEventArgs(this.replayArgs!.GameEnd.Teams)
#else
            var args = new GameEndDisplayEventArgs(this.replayArgs!.GameEnd.Players)
#endif
            {
#if HACKATHON
                ReplayArgs = this.replayArgs,
#endif
            };
            ChangeWithoutStack<GameEnd>(args);
        }
        else if (this.replayTick != lastReplayIndex)
        {
            this.UpdateReplayTick(this.replayTick);
        }
    }

    private void UpdateReplayTick(int tick)
    {
        var gameState = this.replayGameStates[tick];
        this.updater.UpdateTimer(gameState.Tick);
        this.updater.UpdateGrid(gameState);

#if STEREO
        this.updater.UpdateTeams(gameState.Teams);
        this.updater.RefreshTeamBarPanels(gameState.Teams);
#else
        this.updater.UpdatePlayers(gameState.Players);
        this.updater.RefreshPlayerBarPanels();
#endif
    }

    private async void Game_Showing(object? sender, SceneDisplayEventArgs? e)
    {
        this.isReplay = e is ReplaySceneDisplayEventArgs;
        this.replayArgs = e as ReplaySceneDisplayEventArgs;

        if (!this.isReplay)
        {
            ServerConnection.MessageReceived += this.Connection_MessageReceived;
        }

#if DEBUG

        if (!this.isReplay)
        {
            var lobbyDataRequest = new EmptyPayload() { Type = PacketType.LobbyDataRequest };
            var packet = PacketSerializer.Serialize(lobbyDataRequest);
            await ServerConnection.SendAsync(packet);
        }

#endif

        this.components.MenuButton.IsEnabled = !this.isReplay
#if HACKATHON
            || (!this.replayArgs?.ShowMode ?? true)
#endif
        ;

        if (!this.IsContentLoaded)
        {
            this.isContentLoading = true;
            ShowOverlay<Loading>();
            await Task.Run(this.LoadContent);
            HideOverlay<Loading>();
            this.isContentLoading = false;
        }

        if (this.isReplay)
        {
            try
            {
                ShowOverlay<Loading>();

                await Task.Run(() =>
                {
                    var replay = (e as ReplaySceneDisplayEventArgs)!;
                    this.replayGameStates = replay.GameStates;
                    this.updater.EnableGridComponent();
                    this.UpdateReplayTick(0);
                    this.isReplayLoaded = true;
                });
            }
            catch (Exception ex)
            {
                DebugConsole.ThrowError("Failed to load replay data.");
                DebugConsole.ThrowError(ex);
                ChangeWithoutStack<Replay.ChooseReplay>();
                return;
            }
            finally
            {
                HideOverlay<Loading>();
            }
        }

#if HACKATHON
        else
        {
            string? matchName = Settings?.MatchName;
            if (Settings is not null && Settings.SandboxMode)
            {
                if (matchName is null)
                {
                    matchName = "sandbox";
                }
                else
                {
                    matchName += " [sandbox]";
                }
            }

            this.updater.UpdateMatchName(matchName);
        }
#endif

        if (!this.isReplay)
        {
            var gameSceneLoadedPayload = new EmptyPayload() { Type = PacketType.ReadyToReceiveGameState };
            var packet = PacketSerializer.Serialize(gameSceneLoadedPayload);
            await ServerConnection.SendAsync(packet);
        }
    }

    private async void Game_Hiding(object? sender, EventArgs e)
    {
        this.updater.DisableGridComponent();

        /* TODO: Unload content */

        this.components.Grid.ClearSprites();

#if !STEREO
        this.components.PlayerIdentityBarPanel.Clear();
        this.components.PlayerStatsBarPanel.Clear();
#endif

#if HACKATHON
        this.updater.UpdateMatchName(null);
#endif
        this.updater.UpdateTimer(0);

        if (ServerConnection.IsConnected)
        {
            await ServerConnection.CloseAsync();
        }

        ServerConnection.MessageReceived -= this.Connection_MessageReceived;
    }

    private void Game_Hid(object? sender, EventArgs e)
    {
#if STEREO
        this.teams.Clear();
#else
        this.players.Clear();
#endif
        this.replayGameStates = [];
        this.replayTick = -1;
        this.isReplay = false;
        this.isReplayLoaded = false;
        this.isReplayRunning = false;
        this.replayTime = TimeSpan.Zero;
        this.replayInputHandler.Reset();

#if STEREO
        this.updater.ResetTeamBarPanels();
#endif

        if (Current is not GameEnd)
        {
            this.replayArgs?.Dispose();
        }
    }
}
