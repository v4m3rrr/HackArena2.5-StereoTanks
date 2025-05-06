using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.UI.LobbySceneComponents;

#if STEREO

/// <summary>
/// Represents a team slot panel.
/// </summary>
internal class TeamSlotPanel : FlexListBox
{
    private readonly RoundedSolidColor background;
    private readonly ScalableTexture2D waitingIcon;
    private readonly PlayerSlotPanel lightPlayerSlot;
    private readonly PlayerSlotPanel heavyPlayerSlot;
    private readonly RoundedSolidColor teamColor;
    private readonly LocalizedText teamName;

    private Team? team;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamSlotPanel"/> class.
    /// </summary>
    public TeamSlotPanel()
        : base()
    {
        this.background = new RoundedSolidColor(GameClientCore.ThemeColor, 25)
        {
            Parent = this,
            AutoAdjustRadius = true,
            Opacity = 0.35f,
        };

        var teamLabelContainer = new Container()
        {
            Parent = this.ContentContainer,
        };

        this.SetResizeFactor(teamLabelContainer, 0.85f);

        var teamColorContainer = new Container()
        {
            Parent = teamLabelContainer,
            Transform =
            {
                RelativeSize = new Vector2(0.16f, 1f),
                Alignment = Alignment.Left,
                Ratio = new Ratio(1, 1),
            },
        };

        this.waitingIcon = new ScalableTexture2D("Images/Icons/waiting.svg")
        {
            Parent = teamColorContainer,
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                RelativeSize = new Vector2(0.35f),
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };

        this.teamColor = new RoundedSolidColor(Color.White, int.MaxValue)
        {
            Parent = teamColorContainer,
            IsEnabled = false,
            Transform =
            {
                RelativeSize = new Vector2(0.3f),
                Alignment = Alignment.Center,
            },
        };

        var font = new ScalableFont(Styles.Fonts.Paths.Main, 18)
        {
            AutoResize = true,
            Spacing = 5,
        };

        var teamNameContainer = new Container()
        {
            Parent = teamLabelContainer,
            Transform =
            {
                RelativeSize = new Vector2(0.85f, 1f),
                Alignment = Alignment.Right,
            },
        };

        this.teamName = new LocalizedText(font, Color.White)
        {
            Parent = teamNameContainer,
            Value = GetWaitingText(),
            Case = TextCase.Upper,
            TextAlignment = Alignment.Left,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.Left,
            },
        };

        this.lightPlayerSlot = new PlayerSlotPanel(TankType.Light)
        {
            Parent = this.ContentContainer,
        };

        this.heavyPlayerSlot = new PlayerSlotPanel(TankType.Heavy)
        {
            Parent = this.ContentContainer,
        };
    }

    /// <summary>
    /// Gets or sets the team the panel represents.
    /// </summary>
    public Team? Team
    {
        get => this.team;
        set
        {
            if (this.team == value)
            {
                return;
            }

            this.team = value;

            if (value is null)
            {
                this.teamColor.IsEnabled = false;
                this.waitingIcon.IsEnabled = true;
                this.teamName.Value = GetWaitingText();
            }
            else
            {
                this.teamColor.IsEnabled = true;
                this.teamColor.Color = new Color(value.Color);
                this.waitingIcon.IsEnabled = false;
                (this.teamName as Text).Value = value.Name;
                this.lightPlayerSlot.Player = value.Players.FirstOrDefault(p => p.Tank.Type is TankType.Light);
                this.heavyPlayerSlot.Player = value.Players.FirstOrDefault(p => p.Tank.Type is TankType.Heavy);
            }
        }
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (this.waitingIcon.IsEnabled)
        {
            this.waitingIcon.Rotation += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
        }

        base.Update(gameTime);
    }

    private static FormattedLocalizedString GetWaitingText()
    {
        return new FormattedLocalizedString("Other.Waiting")
        {
            Suffix = "...",
        };
    }
}

#endif
