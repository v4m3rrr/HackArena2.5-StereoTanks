using System;
using System.Linq;
using System.Threading.Tasks;
using GameClient.Networking;
using GameClient.Scenes.CoopCore;
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
internal class Coop : Scene
{
    private readonly CoopInitializer initializer;
    private readonly CoopComponents components;

    /// <summary>
    /// Initializes a new instance of the <see cref="Coop"/> class.
    /// </summary>
    public Coop()
    {
        this.initializer = new CoopInitializer(this);
        this.components = new CoopComponents(this.initializer);
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

        var difficulty = this.components.DifficultySelector.SelectedItem?.TValue ?? Difficulty.Easy;

        string input = this.GetAddress();
        string[] parts = input.Split(':');
        string host = parts[0];
        string port = parts.Length > 1 ? parts[1] : "";

        GameClientCore.Bots.Add(new Bot("Bots", TankType.Heavy, difficulty, host, port));
        GameClientCore.Bots.Add(new Bot("Bots", TankType.Light, difficulty, host, port));

        GameClientCore.Server.Start(host, port);
        foreach (var bot in GameClientCore.Bots)
        {
            await bot.Start(); // Stop any previous bots before starting new ones
        }

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
        this.components.TankTypeSelector.SelectItem((item) => item.TValue == CoopData.TankType);
        this.components.DifficultySelector.SelectItem((item) => item.TValue == CoopData.Difficulty);
#else
        this.components.NicknameInput.SetText(JoinData.Nickname ?? string.Empty);
#endif
    }

    private void JoinRoom_Hiding(object? sender, EventArgs? e)
    {
#if STEREO
        var tankType = this.components.TankTypeSelector.SelectedItem?.TValue;
        CoopData.TankType = tankType ?? TankType.Light;

        var difficulty = this.components.DifficultySelector.SelectedItem?.TValue;
        CoopData.Difficulty = difficulty ?? Difficulty.Easy;
#else
        var nickname = this.components.NicknameInput.Value;
        SinglePlayerData.Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname;
#endif

        _ = CoopData.Save();
    }

#if STEREO

    private string GetTeamName()
    {
        return this.components.TeamName;
    }

    private TankType GetTankType()
    {
        return this.components.TankTypeSelector.SelectedItem!.TValue;
    }

#endif

    private string? GetJoinCode()
    {
        return null;
    }

    private string GetAddress()
    {
        return this.components.AddressSection.GetDescendant<TextInput>()!.Value;
    }
}
