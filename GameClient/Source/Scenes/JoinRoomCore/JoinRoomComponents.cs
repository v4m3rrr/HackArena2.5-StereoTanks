using GameLogic;
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

#if STEREO
        this.TeamNameSection = initializer.CreateSectionWithTextInput(this.BaseListBox, new LocalizedString("Labels.TeamName"), 24);
        this.TeamNameInput = this.TeamNameSection.GetDescendant<TextInput>()!;
#else
        this.NicknameSection = initializer.CreateSectionWithTextInput(this.BaseListBox, new LocalizedString("Labels.Nickname"), 24);
        this.NicknameInput = this.NicknameSection.GetDescendant<TextInput>()!;
#endif

#if STEREO
        this.TankTypeSection = initializer.CreateTankTypeSection(this.BaseListBox, new LocalizedString("Labels.TankType"));
        this.TankTypeSelector = this.TankTypeSection.GetDescendant<Selector<TankType>>()!;
#endif

        this.RoomCodeSection = initializer.CreateSectionWithTextInput(this.BaseListBox, new LocalizedString("Labels.RoomCode"), 8);

        this.AddressSection = initializer.CreateSectionWithTextInput(this.BaseListBox, new LocalizedString("Labels.ServerAddress"), 24);
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

#if STEREO

    /// <summary>
    /// Gets the team name section component.
    /// </summary>
    public Container TeamNameSection { get; }

#else

    /// <summary>
    /// Gets the nickname section component.
    /// </summary>
    public Container NicknameSection { get; }

#endif

#if STEREO

    /// <summary>
    /// Gets the tank type section component.
    /// </summary>
    public Container TankTypeSection { get; }

    /// <summary>
    /// Gets the tank type selector component.
    /// </summary>
    public Selector<TankType> TankTypeSelector { get; }

#endif

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

#if STEREO

    /// <summary>
    /// Gets the team name text input component.
    /// </summary>
    public TextInput TeamNameInput { get; }

#else

    /// <summary>
    /// Gets the nickname text input component.
    /// </summary>
    public TextInput NicknameInput { get; }

#endif

    /// <summary>
    /// Gets the address text input component.
    /// </summary>
    public TextInput AddressInput { get; }
}
