using MonoRivUI;

namespace GameClient.Scenes.JoinRoomCore;

/// <summary>
/// Represents the join room components.
/// </summary>
internal class JoinRoomComponents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinRoomComponents"/> class.
    /// </summary>
    /// <param name="initializer">
    /// The join room initializer that will be used
    /// to create the join room components.
    /// </param>
    public JoinRoomComponents(JoinRoomInitializer initializer)
    {
        this.RoomText = initializer.CreateRoomText();
        this.BaseListBox = initializer.CreateBaseListBox();

        this.NicknameSection = initializer.CreateSection(this.BaseListBox, new LocalizedString("Labels.Nickname"), 24);
        this.NicknameInput = this.NicknameSection.GetDescendant<TextInput>()!;

        this.RoomCodeSection = initializer.CreateSection(this.BaseListBox, new LocalizedString("Labels.RoomCode"), 8);

        this.AddressSection = initializer.CreateSection(this.BaseListBox, new LocalizedString("Labels.ServerAddress"), 24);
        this.AddressInput = this.AddressSection.GetDescendant<TextInput>()!;

        this.JoinButton = initializer.CreateJoinButton();
        this.BackButton = initializer.CreateBackButton();
        this.SpectateButton = initializer.CreateSpectateButton();
    }

    /// <summary>
    /// Gets the room text component.
    /// </summary>
    public LocalizedText RoomText { get; }

    /// <summary>
    /// Gets the base list box component.
    /// </summary>
    public FlexListBox BaseListBox { get; }

    /// <summary>
    /// Gets the nickname section component.
    /// </summary>
    public Container NicknameSection { get; }

    /// <summary>
    /// Gets the room code section component.
    /// </summary>
    public Container RoomCodeSection { get; }

    /// <summary>
    /// Gets the address section component.
    /// </summary>
    public Container AddressSection { get; }

    /// <summary>
    /// Gets the join button component.
    /// </summary>
    public Button<Container> JoinButton { get; }

    /// <summary>
    /// Gets the back button component.
    /// </summary>
    public Button<Container> BackButton { get; }

    /// <summary>
    /// Gets the spectate button component.
    /// </summary>
    public Button<Container> SpectateButton { get; }

    /// <summary>
    /// Gets the nickname text input component.
    /// </summary>
    public TextInput NicknameInput { get; }

    /// <summary>
    /// Gets the address text input component.
    /// </summary>
    public TextInput AddressInput { get; }
}
