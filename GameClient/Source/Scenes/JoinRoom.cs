using GameClient.Networking;
using GameClient.Scenes.JoinRoomCore;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the join room scene.
/// </summary>
internal class JoinRoom : Scene
{
    private readonly JoinRoomComponents components;

    /// <summary>
    /// Initializes a new instance of the <see cref="JoinRoom"/> class.
    /// </summary>
    public JoinRoom()
    {
        var initializer = new JoinRoomInitializer(this);
        this.components = new JoinRoomComponents(initializer);
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

    public async void JoinGame()
    {
        var nickname = this.components.NickNameSection.GetDescendant<TextInput>()!.Value;
        string? joinCode = this.components.RoomCodeSection.GetDescendant<TextInput>()!.Value;
        var address = this.components.AddressSection.GetDescendant<TextInput>()!.Value;

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            joinCode = null;
        }

#if DEBUG
        var connectionData = ConnectionData.ForPlayer(address, joinCode, nickname, false);
#else
        var connectionData = ConnectionData.ForPlayer(address, joinCode, nickname);
#endif

        ServerConnection.ErrorThrew += DebugConsole.ThrowError;
        await ServerConnection.ConnectAsync(connectionData);
        ServerConnection.ErrorThrew -= DebugConsole.ThrowError;
        Change<Lobby>();
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showed += this.JoinRoom_Showed;
    }

    private static void UpdateMainMenuBackgroundEffectRotation(GameTime gameTime)
    {
        MainMenu.Effect.Rotation += 0.1f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        MainMenu.Effect.Rotation %= MathHelper.TwoPi;
    }

    private void JoinRoom_Showed(object? sender, SceneDisplayEventArgs? e)
    {
        this.components.AddressSection.GetDescendant<TextInput>()!.SetText(GameSettings.ServerAddress);
    }
}
