using GameLogic;
using MonoRivUI;

namespace GameClient.Scenes.SinglePlayerCore;

/// <summary>
/// Represents the join room components.
/// </summary>
internal class SinglePlayerComponents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SinglePlayerComponents"/> class.
    /// </summary>
    /// <param name="initializer">
    /// The join room initializer that will be used
    /// to create the join room components.
    /// </param>
    public SinglePlayerComponents(SinglePlayerInitializer initializer)
    {
        this.RoomText = initializer.CreateRoomText();
        this.BaseListBox = initializer.CreateBaseListBox();
        this.Address = initializer.GetAddress();

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

        this.DifficultySection = initializer.CreateDiffcultySection(this.BaseListBox, new LocalizedString("Labels.Difficulty"));
        this.DifficultySelector = this.DifficultySection.GetDescendant<Selector<Difficulty>>()!;
#endif

        this.JoinButton = initializer.CreateJoinButton();
        this.BackButton = initializer.CreateBackButton();
    }

    /// <summary>
    /// Gets the room text component.
    /// </summary>
    public LocalizedText RoomText { get; }

    /// <summary>
    /// Gets the address of local server.
    /// </summary>
    public string Address { get; }

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

    /// <summary>
    /// Gets the tank type section component.
    /// </summary>
    public Container DifficultySection { get; }

    /// <summary>
    /// Gets the tank type selector component.
    /// </summary>
    public Selector<Difficulty> DifficultySelector { get; }

#endif

    /// <summary>
    /// Gets the join button component.
    /// </summary>
    public Button<Container> JoinButton { get; }

    /// <summary>
    /// Gets the back button component.
    /// </summary>
    public Button<Container> BackButton { get; }

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

}
