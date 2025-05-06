using System;
using System.Linq;
using System.Threading.Tasks;
using GameClient.Networking;
using GameClient.Scenes.JoinRoomCore;
using GameClient.UI;
using GameLogic;
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
        var address = this.GetAddress();
        var joinCode = this.GetJoinCode();

        var data = new ConnectionPlayerData(address, joinCode)
        {
#if STEREO
            TeamName = this.GetTeamName(),
            TankType = this.GetTankType(),
#else
            Nickname = this.GetNickname(),
#endif
        };

        await Join(data);
    }

    /// <summary>
    /// Connects to the server and joins the game as a spectator.
    /// </summary>
    public async void JoinGameAsSpectator()
    {
        var address = this.GetAddress();
        var joinCode = this.GetJoinCode();
        var data = new ConnectionSpectatorData(address, joinCode);
        await Join(data);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += this.JoinRoom_Showing;
        this.Hiding += this.JoinRoom_Hiding;
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

        var cancelationTokenSource = new System.Threading.CancellationTokenSource();
        var cancelationToken = cancelationTokenSource.Token;

        var connectingMessageBox = new ConnectingMessageBox(cancelationTokenSource);
        ScreenController.ShowOverlay(connectingMessageBox);

        ConnectionStatus status = await ServerConnection.ConnectAsync(data, cancelationToken);
        ServerConnection.ErrorThrew -= DebugConsole.ThrowError;

        switch (status)
        {
            case ConnectionStatus.Success:
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

        ScreenController.HideOverlay(connectingMessageBox);
    }

    private void JoinRoom_Showing(object? sender, SceneDisplayEventArgs? e)
    {
#if STEREO
        this.components.TeamNameInput.SetText(JoinData.TeamName ?? string.Empty);
        this.components.TankTypeSelector.SelectItem((item) => item.TValue == JoinData.TankType);
#else
        this.components.NicknameInput.SetText(JoinData.Nickname ?? string.Empty);
#endif

        this.components.AddressInput.SetText(JoinData.Address ?? JoinData.DefaultAddress);
    }

    private void JoinRoom_Hiding(object? sender, EventArgs? e)
    {
#if STEREO
        var teamName = this.components.TeamNameInput.Value;
        JoinData.TeamName = string.IsNullOrWhiteSpace(teamName) ? null : teamName;

        var tankType = this.components.TankTypeSelector.SelectedItem?.TValue;
        JoinData.TankType = tankType ?? TankType.Light;
#else
        var nickname = this.components.NicknameInput.Value;
        JoinData.Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname;
#endif

        var address = this.components.AddressInput.Value;
        JoinData.Address = string.IsNullOrWhiteSpace(address)
            || address == JoinData.DefaultAddress ? null : address;

        _ = JoinData.Save();
    }

#if STEREO

    private string GetTeamName()
    {
        return this.components.TeamNameSection.GetDescendant<TextInput>()!.Value;
    }

    private TankType GetTankType()
    {
        return this.components.TankTypeSelector.SelectedItem!.TValue;
    }

#else

    private string GetNickname()
    {
        return this.components.NicknameSection.GetDescendant<TextInput>()!.Value;
    }

#endif

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
