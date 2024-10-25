using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using GameClient.Networking;
using GameClient.Scenes.GameCore;
using GameClient.Scenes.GameOverlays;
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
    private readonly Dictionary<string, Player> players = [];
    private readonly GameComponents components;
    private readonly GameUpdater updater;

    private bool isContentLoading;
    private bool isContentUpdatedAfterLoad;

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game()
        : base(Color.Transparent)
    {
        var initializer = new GameInitializer(this);
        this.components = new GameComponents(initializer);
        this.updater = new GameUpdater(this.components, this.players);
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
            HandleInput();
        }

        if (this.IsContentLoaded)
        {
            this.isContentUpdatedAfterLoad = true;
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

        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += this.Game_Showing;
        this.Hiding += this.Game_Hiding;
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        var textures = this.BaseComponent.GetAllDescendants<TextureComponent>();
        textures.ToList().ForEach(x => x.Load());

        GameSceneComponents.PlayerBarComponent.LoadContent();

        Sprites.Bullet.LoadContent();
        Sprites.DoubleBullet.LoadContent();
        Sprites.FogOfWar.LoadContent();
        Sprites.Mine.LoadContent();
        Sprites.SecondaryItem.LoadContent();
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
            MainEffect.Rotation += Math.Min(MainEffect.Rotation, value) * sign;
            MainEffect.Rotation %= MathHelper.TwoPi;
        }
    }

    private static async void HandleInput()
    {
        var payload = GameInputHandler.HandleInputPayload();

        if (ServerConnection.Data.IsSpectator && payload is IActionPayload)
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

    private async void Game_Showing(object? sender, SceneDisplayEventArgs? e)
    {
        ServerConnection.BufferSize = 1024 * 32;
        ServerConnection.MessageReceived += this.Connection_MessageReceived;

        if (!this.IsContentLoaded)
        {
            this.isContentLoading = true;
            ShowOverlay<Loading>();
            await Task.Run(this.LoadContent);
            HideOverlay<Loading>();
            this.isContentLoading = false;
        }

        var gameSceneLoadedPayload = new EmptyPayload() { Type = PacketType.ReadyToReceiveGameState };
        var packet = PacketSerializer.Serialize(gameSceneLoadedPayload);
        await ServerConnection.SendAsync(packet);

        if (Settings is null)
        {
            var lobbyDataRequest = new EmptyPayload() { Type = PacketType.LobbyDataRequest };
            packet = PacketSerializer.Serialize(lobbyDataRequest);
            await ServerConnection.SendAsync(packet);
        }
#if HACKATHON
        else
        {
            string? matchName = Settings.MatchName;
            if (Settings.SandboxMode)
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
    }

    private async void Game_Hiding(object? sender, EventArgs e)
    {
        this.components.Grid.ResetAllFogsOfWar();
        this.updater.DisableGridComponent();

        /* TODO: Unload content */

        this.components.Grid.ClearSprites();
        this.components.PlayerIdentityBarPanel.Clear();
        this.components.PlayerStatsBarPanel.Clear();

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
}
