using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using GameClient.Networking;
using GameClient.Scenes.GameCore;
using GameClient.Scenes.LobbyCore;
using GameLogic.Networking;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the lobby scene.
/// </summary>
[AutoInitialize]
[AutoLoadContent]
internal class Lobby : Scene
{
    private readonly LobbyComponents components;
    private readonly LobbyUpdater updater;

    /// <summary>
    /// Initializes a new instance of the <see cref="Lobby"/> class.
    /// </summary>
    public Lobby()
        : base(Color.Transparent)
    {
        var initializer = new LobbyInitializer(this);
        this.components = new LobbyComponents(initializer);
        this.updater = new LobbyUpdater(this.components);
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
        MainEffect.Draw();
        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += this.Lobby_Showing;
        this.Hiding += this.Lobby_Hiding;
        this.Hid += this.Lobby_Hid;
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        var textures = this.BaseComponent.GetAllDescendants<TextureComponent>();
        textures.ToList().ForEach(x => x.Load());
    }

    private static void UpdateMainMenuBackgroundEffectRotation(GameTime gameTime)
    {
        MainEffect.Rotation -= 0.1f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        MainEffect.Rotation %= MathHelper.TwoPi;
    }

    private static void Connection_ErrorThrew(string error)
    {
        DebugConsole.ThrowError(error);
        ChangeToPreviousOrDefault<MainMenu>();
    }

    private static async Task SendLobbyDataRequest()
    {
        var payload = new EmptyPayload() { Type = PacketType.LobbyDataRequest };
        var packet = PacketSerializer.Serialize(payload);
        await ServerConnection.SendAsync(packet);
    }

    private static async Task SendGameStatusRequest()
    {
        var payload = new EmptyPayload() { Type = PacketType.GameStatusRequest };
        var packet = PacketSerializer.Serialize(payload);
        await ServerConnection.SendAsync(packet);
    }

    private void Lobby_Showing(object? sender, SceneDisplayEventArgs? e)
    {
        this.updater.UpdateJoinCode();

        ServerConnection.MessageReceived += this.Connection_MessageReceived;
        ServerConnection.ErrorThrew += Connection_ErrorThrew;

        _ = Task.Run(async () =>
        {
            await SendLobbyDataRequest();
            await SendGameStatusRequest();
        });
    }

    private void Lobby_Hiding(object? sender, EventArgs e)
    {
        ServerConnection.MessageReceived -= this.Connection_MessageReceived;
        ServerConnection.ErrorThrew -= Connection_ErrorThrew;
    }

    private void Lobby_Hid(object? sender, EventArgs e)
    {
        this.updater.ResetPlayerSlotPanels();
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

                case PacketType.LobbyData:
                    LobbyServerMessageHandler.HandleLobbyDataPacket(packet, this.updater);
                    break;

                case PacketType.GameStarting:
                case PacketType.GameInProgress when ServerConnection.Data.IsSpectator:
                case PacketType.GameInProgress when Game.Settings?.SandboxMode ?? false:
                case PacketType.GameStarted when ServerConnection.Data.IsSpectator:
                    Change<Game>();
                    break;

                default:
                    if (!packet.Type.IsGroup(PacketType.ErrorGroup))
                    {
                        DebugConsole.SendMessage(
                        $"Unknown packet type in Lobby scene: {packet.Type}",
                        Color.Yellow);
                    }

                    break;
            }
        }
    }
}
