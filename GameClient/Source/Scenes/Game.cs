using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using GameLogic;
using GameLogic.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient.Scenes;

/// <summary>
/// Represents the game scene.
/// </summary>
internal class Game : Scene
{
    private readonly ClientWebSocket client = new();
    private readonly GridComponent grid;
    private DateTime? lastPingSend;

    private Text pingInfo = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game()
        : base(Color.DimGray)
    {
        this.client = new ClientWebSocket();

        this.grid = new GridComponent(new Grid())
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
                RelativeSize = new Vector2(0.95f),
            },
        };
    }

    /// <summary>
    /// Gets or sets the server URI.
    /// </summary>
    public static Uri ServerUri { get; set; } = new("ws://localhost:5000");

    /// <summary>
    /// Gets the server broadcast interval.
    /// </summary>
    /// <value>
    /// The server broadcast interval in seconds.
    /// When the value is -1, the server broadcast interval is not received yet.
    /// </value>
    public static float ServerBroadcastInterval { get; private set; } = -1f;

    /// <inheritdoc/>
    public override async void Update(GameTime gameTime)
    {
        this.HandleInput();

        base.Update(gameTime);

        var shouldSendPing = (this.lastPingSend?.AddSeconds(1) ?? DateTime.UtcNow) <= DateTime.UtcNow;
        if (this.client.State == WebSocketState.Open && shouldSendPing)
        {
            await this.SendPingAsync();
        }
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += async (s, e) =>
        {
            DebugConsole.SendMessage("Connecting to server...");
            await this.ConnectAsync(ServerUri);
            DebugConsole.SendMessage($"Connected to server {ServerUri}.");
        };

        var backBtn = new Button<Frame>(new Frame())
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.04f, -0.04f),
                RelativeSize = new Vector2(0.12f, 0.07f),
            },
        }.ApplyStyle(Styles.UI.ButtonStyle);
        backBtn.Clicked += (s, e) => ChangeToPreviousOr<MainMenu>();
        backBtn.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.MainMenu");

        var settingsBtn = new Button<Frame>(new Frame())
        {
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.BottomLeft,
                RelativeOffset = new Vector2(0.04f, -0.12f),
                RelativeSize = new Vector2(0.12f, 0.07f),
            },
        }.ApplyStyle(Styles.UI.ButtonStyle);
        settingsBtn.Clicked += (s, e) => ShowOverlay<Settings>(new OverlayShowOptions(BlockFocusOnUnderlyingScenes: true));
        settingsBtn.GetDescendant<LocalizedText>()!.Value = new LocalizedString("Buttons.Settings");

        this.pingInfo = new Text(new ScalableFont("Content\\Fonts\\verdana.ttf", 11), Color.Black)
        {
            Parent = this.BaseComponent,
            TextAlignment = Alignment.TopRight,
            Value = "Ping: 0 ms",
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.TopRight,
                RelativeOffset = new Vector2(-0.04f, 0.04f),
            },
        };
    }

    private async Task ConnectAsync(Uri server)
    {
        await this.client.ConnectAsync(server, CancellationToken.None);
        _ = Task.Run(this.ReceiveMessages);

        await this.RequestGameDataAsync();
    }

    private async Task RequestGameDataAsync()
    {
        try
        {
            var packet = new EmptyPayload() { Type = PacketType.GameData };
            var buffer = PacketSerializer.ToByteArray(packet);
            await this.client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            DebugConsole.SendMessage("An error occurred while requesting game data: " + e.Message, Color.Red);
            throw;
        }
    }

    private async Task ReceiveMessages()
    {
        while (this.client.State == WebSocketState.Open)
        {
            try
            {
                var buffer = new byte[1024 * 32];
                WebSocketReceiveResult result = await this.client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    Packet packet = PacketSerializer.Deserialize(buffer);
                    switch (packet.Type)
                    {
                        case PacketType.Ping:
                            var responseTime = DateTime.UtcNow - this.lastPingSend!.Value;
                            this.pingInfo.Value = $"Ping: {responseTime.TotalMilliseconds:0.00} ms";
                            break;

                        case PacketType.GridData:
                            var gridData = packet.GetPayload<GridStatePayload>();
                            this.grid.Logic.UpdateFromPayload(gridData);

                            break;

                        case PacketType.GameData:
                            var gameData = packet.GetPayload<GameStatePayload>();
                            DebugConsole.SendMessage("Game ID: " + gameData.Id);
                            DebugConsole.SendMessage("Join code: " + gameData.JoinCode);
                            ServerBroadcastInterval = gameData.BroadcastInterval;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                DebugConsole.SendMessage("An error occurred while receiving messages: " + e.Message, Color.Red);
                return;
            }
        }
    }

    private async Task SendPingAsync()
    {
        try
        {
            this.lastPingSend = DateTime.UtcNow;
            var packet = new EmptyPayload() { Type = PacketType.Ping };
            var buffer = PacketSerializer.ToByteArray(packet);
            await this.client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception e)
        {
            DebugConsole.SendMessage("An error occurred while sending a ping: " + e.Message, Color.Red);
            throw;
        }
    }

    // TODO: Refactor!!!
    private async void HandleInput()
    {
        IPacketPayload? payload = null;
        if (KeyboardController.IsKeyHit(Keys.W))
        {
            payload = new TankMovementPayload(TankMovement.Forward);
        }
        else if (KeyboardController.IsKeyHit(Keys.S))
        {
            payload = new TankMovementPayload(TankMovement.Backward);
        }
        else if (KeyboardController.IsKeyHit(Keys.A))
        {
            payload = new TankRotationPayload() { TankRotation = Rotation.Left };
        }
        else if (KeyboardController.IsKeyHit(Keys.D))
        {
            payload = new TankRotationPayload() { TankRotation = Rotation.Right };
        }
        else if (KeyboardController.IsKeyHit(Keys.Space))
        {
            payload = new TankShootPayload();
        }
#if DEBUG
        else if (KeyboardController.IsKeyHit(Keys.T))
        {
            payload = new EmptyPayload() { Type = PacketType.ShootAll };
        }
#endif

        var p = payload as TankRotationPayload;
        if (KeyboardController.IsKeyHit(Keys.Q))
        {
            payload = new TankRotationPayload() { TankRotation = p?.TankRotation, TurretRotation = Rotation.Left };
        }
        else if (KeyboardController.IsKeyHit(Keys.E))
        {
            payload = new TankRotationPayload() { TankRotation = p?.TankRotation, TurretRotation = Rotation.Right };
        }

        if (payload is not null)
        {
            var buffer = PacketSerializer.ToByteArray(payload);
            await this.client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
