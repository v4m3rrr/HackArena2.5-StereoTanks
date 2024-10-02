using GameClient.UI.SceneComponents;
using GameLogic;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.LobbySceneComponents;

/// <summary>
/// Represents a player slot panel.
/// </summary>
internal class PlayerSlotPanel : Component
{
    private readonly RoundedSolidColor background;
    private readonly RoundedSolidColor iconBackground;
    private readonly TankSpriteIcon tankSpriteIcon;
    private readonly ScalableTexture2D waitingIcon;
    private readonly Text playerNick;

    private Player? player;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerSlotPanel"/> class.
    /// </summary>
    public PlayerSlotPanel()
    {
        this.background = new RoundedSolidColor(MonoTanks.ThemeColor, 15)
        {
            Parent = this,
            Opacity = 0.35f,
        };

        this.iconBackground = new RoundedSolidColor(Color.White, 15)
        {
            Parent = this,
            Opacity = 0.3f,
            Transform =
            {
                RelativeSize = new Vector2(0.8f),
                Alignment = Alignment.Left,
                RelativeOffset = new Vector2(0.04f, 0.0f),
                Ratio = new Ratio(1, 1),
            },
        };

        this.tankSpriteIcon = new TankSpriteIcon()
        {
            Parent = this.iconBackground,
            IsEnabled = false,
            Transform =
            {
                RelativeSize = new Vector2(0.55f),
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };

        this.waitingIcon = new ScalableTexture2D("Images/waiting_icon.svg")
        {
            Parent = this.iconBackground,
            RelativeOrigin = new Vector2(0.5f),
            CenterOrigin = true,
            Transform =
            {
                RelativeSize = new Vector2(0.35f),
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
            },
        };

        var font = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 19);

        var nickContainer = new Container()
        {
            Parent = this,
            Transform =
            {
                RelativeSize = new Vector2(0.7f, 1f),
                Alignment = Alignment.Right,
            },
        };

        this.playerNick = new LocalizedText(font, Color.White)
        {
            Parent = nickContainer,
            Value = new FormattedLocalizedString("Other.Waiting")
            {
                Suffix = "...",
            },
            Spacing = 8,
            Case = TextCase.Upper,
            TextAlignment = Alignment.Left,
            TextShrink = TextShrinkMode.HeightAndWidth,
            Transform =
            {
                RelativeSize = new Vector2(0.9f, 0.6f),
                Alignment = Alignment.Center,
            },
        };
    }

    /// <summary>
    /// Gets or sets the player.
    /// </summary>
    public Player? Player
    {
        get => this.player;
        set
        {
            if (value == this.player)
            {
                return;
            }

            this.player = value;
            this.playerNick.Value = value?.Nickname ?? "Waiting...";

            this.waitingIcon.IsEnabled = value is null;
            this.tankSpriteIcon.IsEnabled = value is not null;

            if (value is not null)
            {
                this.tankSpriteIcon.SetColor(new Color(value!.Color));
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
}
