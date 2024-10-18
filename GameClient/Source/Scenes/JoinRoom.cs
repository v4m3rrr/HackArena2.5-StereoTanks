using System.Linq;
using System.Threading.Tasks;
using GameClient.Networking;
using GameClient.Scenes.JoinRoomCore;
using GameClient.UI;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the join room scene.
/// </summary>
[AutoInitialize]
[AutoLoadContent]
internal class JoinRoom : Scene
{
    private readonly JoinRoomInitializer initializer;
    private readonly JoinRoomComponents components;

    /// <summary>
    /// Initializes a new instance of the <see cref="JoinRoom"/> class.
    /// </summary>
    public JoinRoom()
    {
        this.initializer = new JoinRoomInitializer(this);
        this.components = new JoinRoomComponents(this.initializer);
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

    /// <summary>
    /// Connects to the server and joins the game.
    /// </summary>
    public async void JoinGame()
    {
        var nickname = this.GetNickname();
        var address = this.GetAddress();
        var joinCode = this.GetJoinCode();

#if DEBUG
        var data = ConnectionData.ForPlayer(address, joinCode, nickname, false);
#else
        var data = ConnectionData.ForPlayer(address, joinCode, nickname);
#endif

        await Join(data);
    }

    /// <summary>
    /// Connects to the server and joins the game as a spectator.
    /// </summary>
    public async void JoinGameAsSpectator()
    {
        var address = this.GetAddress();
        var joinCode = this.GetJoinCode();

#if DEBUG
        var data = ConnectionData.ForSpectator(address, joinCode, false);
#else
        var data = ConnectionData.ForSpectator(address, joinCode);
#endif

        await Join(data);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += this.JoinRoom_Showing;
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        var textures = this.BaseComponent.GetAllDescendants<TextureComponent>();
        textures.ToList().ForEach(x => x.Load());
    }

    private static void UpdateMainMenuBackgroundEffectRotation(GameTime gameTime)
    {
        MainEffect.Rotation += 0.1f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        MainEffect.Rotation %= MathHelper.TwoPi;
    }

    private static async Task Join(ConnectionData data)
    {
        ServerConnection.ErrorThrew += DebugConsole.ThrowError;
        var connectingMessageBox = new ConnectingMessageBox();
        ScreenController.ShowOverlay(connectingMessageBox);

        ConnectionStatus status = await ServerConnection.ConnectAsync(data);

        switch (status)
        {
            case ConnectionStatus.Success:
                ServerConnection.ErrorThrew -= DebugConsole.ThrowError;
                Change<Lobby>();
                break;
            case ConnectionStatus.Failed s:
                ConnectionFailedMessageBox failedMsgBox = s.Exception is null
                    ? new(new LocalizedString("Other.NoDetails"))
                    : s.Exception is System.Net.WebSockets.WebSocketException
                        && s.Exception.Message == "Unable to connect to the remote server"
                        ? new(new LocalizedString("Other.ServerNotResponding"))
                        : new(s.Exception.Message);
                ScreenController.ShowOverlay(failedMsgBox);
                break;
            case ConnectionStatus.Rejected s:
                ScreenController.ShowOverlay(new ConnectionRejectedMessageBox(s.Reason));
                break;
        }

        if (status is ConnectionStatus.Success)
        {
            ServerConnection.ErrorThrew -= DebugConsole.ThrowError;
            Change<Lobby>();
        }
        else if (status is ConnectionStatus.Success)
        {
            var localizedString = new LocalizedString("Other.ServerNotResponding");
            var connectionFailedMessageBox = new ConnectionFailedMessageBox(localizedString);
            ScreenController.ShowOverlay(connectionFailedMessageBox);
        }

        ScreenController.HideOverlay(connectingMessageBox);
    }

    private void JoinRoom_Showing(object? sender, SceneDisplayEventArgs? e)
    {
        this.components.AddressSection.GetDescendant<TextInput>()!.SetText(GameSettings.ServerAddress);
    }

    private string GetNickname()
    {
        return this.components.NickNameSection.GetDescendant<TextInput>()!.Value;
    }

    private string? GetJoinCode()
    {
        string? joinCode = this.components.RoomCodeSection.GetDescendant<TextInput>()!.Value;
        if (string.IsNullOrWhiteSpace(joinCode))
        {
            joinCode = null;
        }

        return joinCode;
    }

    private string GetAddress()
    {
        return this.components.AddressSection.GetDescendant<TextInput>()!.Value;
    }
}
