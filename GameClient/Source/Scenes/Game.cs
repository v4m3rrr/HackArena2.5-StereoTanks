using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using GameClient.Networking;
using GameClient.Scenes.GameCore;
using GameLogic;
using GameLogic.Networking;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the game scene.
/// </summary>
internal class Game : Scene
{
    private readonly Dictionary<string, Player> players = [];
    private readonly GameComponents components;
    private readonly GameUpdater updater;

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
    /// Gets or sets the server broadcast interval in milliseconds.
    /// </summary>
    /// <value>
    /// The server broadcast interval in seconds.
    /// When the value is -1, the server broadcast interval is not received yet.
    /// </value>
    public static int ServerBroadcastInterval { get; set; } = -1;

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
        this.HandleInput();
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
        this.Showing += this.Game_Showing;
        this.Hiding += this.Game_Hiding;
    }

    private static void UpdateMainMenuBackgroundEffectRotation(GameTime gameTime)
    {
        if (MainMenu.Effect.Rotation != 0.0f)
        {
            int sign = MainMenu.Effect.Rotation is > MathHelper.Pi or < 0 and > -MathHelper.Pi ? 1 : -1;
            var value = 0.25f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MainMenu.Effect.Rotation += Math.Min(MainMenu.Effect.Rotation, Math.Min(value, 0.1f)) * sign;
            MainMenu.Effect.Rotation %= MathHelper.TwoPi;
        }
    }

    private static void Connection_Connecting(string server)
    {
        DebugConsole.SendMessage($"Connecting to the server {server}...", Color.LightGreen);
    }

    private static void Connection_Connected()
    {
        DebugConsole.SendMessage("Server status: connected", Color.LightGreen);
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

                case PacketType.GameEnd:
                    GameServerMessageHandler.HandleGameEndPacket(packet);
                    break;

#if DEBUG
                case PacketType.LobbyData:
                    GameServerMessageHandler.HandleLobbyDataPacket(packet, this.updater);
                    break;
#endif

                default:
                    DebugConsole.SendMessage(
                        $"Unknown packet type in Game scene: {packet.Type}",
                        Color.Yellow);
                    break;
            }
        }
    }

    private async void Game_Showing(object? sender, SceneDisplayEventArgs? e)
    {
        if (e is not GameDisplayEventArgs args)
        {
            DebugConsole.ThrowError(
                $"Game scene requires {nameof(GameDisplayEventArgs)}.");
            ChangeToPreviousOrDefault<MainMenu>();
            return;
        }

        ServerConnection.BufferSize = 1024 * 32;
        ServerConnection.MessageReceived += this.Connection_MessageReceived;
        ServerConnection.Connecting += Connection_Connecting;
        ServerConnection.Connected += Connection_Connected;
    }

    private async void Game_Hiding(object? sender, EventArgs e)
    {
        this.components.Grid.ResetAllFogsOfWar();
        this.updater.DisableGridComponent();

        if (ServerConnection.IsConnected)
        {
            await ServerConnection.CloseAsync();
        }

        ServerConnection.MessageReceived -= this.Connection_MessageReceived;
        ServerConnection.Connecting -= Connection_Connecting;
        ServerConnection.Connected -= Connection_Connected;
    }

    private async void HandleInput()
    {
        var payload = GameInputHandler.HandleInputPayload();

        if (payload is not null)
        {
            var message = PacketSerializer.Serialize(payload);
            await ServerConnection.SendAsync(message);
        }
    }
}
