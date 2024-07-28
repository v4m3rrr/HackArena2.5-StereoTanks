using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    private readonly Dictionary<string, Player> players = [];
    private readonly GridComponent grid;

    private ClientWebSocket client;

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game()
        : base(Color.DimGray)
    {
        this.client = new ClientWebSocket();

        this.grid = new GridComponent(new Grid())
        {
            IsEnabled = false,
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
    public static Uri ServerUri { get; set; } = new("ws://127.0.0.1:5000");

    /// <summary>
    /// Gets the server broadcast interval in milliseconds.
    /// </summary>
    /// <value>
    /// The server broadcast interval in seconds.
    /// When the value is -1, the server broadcast interval is not received yet.
    /// </value>
    public static int ServerBroadcastInterval { get; private set; } = -1;

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        this.HandleInput();
        base.Update(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += async (s, e) =>
        {
            await this.ConnectAsync(ServerUri);
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
    }

    private async Task ConnectAsync(Uri server)
    {
        DebugConsole.SendMessage($"Connecting to the server...");
#if DEBUG
        DebugConsole.SendMessage($"Server URI: {server}", Color.DarkGray);
#endif

        int timeout = 5;
        using (HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(timeout) })
        {
            HttpResponseMessage response;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
                var srvUri = new Uri(server.ToString().Replace("ws://", "http://"));
                response = await httpClient.GetAsync(srvUri, cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                DebugConsole.ThrowError("The request timed out.");
                ChangeToPreviousOr<MainMenu>();
                return;
            }
            catch (Exception ex)
            {
                DebugConsole.ThrowError($"An error occurred while sending HTTP request: {ex.Message}");
                ChangeToPreviousOr<MainMenu>();
                return;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.TooManyRequests)
            {
                string errorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                DebugConsole.ThrowError($"Server error: {errorMessage}");
                ChangeToPreviousOr<MainMenu>();
                return;
            }
            else if (response.StatusCode != HttpStatusCode.OK)
            {
                DebugConsole.ThrowError($"Unexpected response from server: {response.StatusCode}");
                ChangeToPreviousOr<MainMenu>();
                return;
            }
        }

        this.client = new ClientWebSocket();

        try
        {
            await this.client.ConnectAsync(server, CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            DebugConsole.ThrowError($"An error occurred while connecting to the server: {ex.Message}");
            ChangeToPreviousOr<MainMenu>();
            return;
        }

        _ = Task.Run(this.ReceiveMessages);
        DebugConsole.SendMessage("Server status: connected", Color.LightGreen);
    }

    private async Task ReceiveMessages()
    {
        while (this.client.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            WebSocketReceiveResult? result = null;
            byte[] buffer = new byte[1024 * 32];
            try
            {
                result = await this.client.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    WebSocketCloseStatus? status = result.CloseStatus;
                    string? description = result.CloseStatusDescription;

                    var msg = description is null
                        ? $"Server status: connection closed ({(int?)status ?? -1})"
                        : $"Server status: connection closed ({(int?)status ?? -1}) - {description}";

                    if (status == WebSocketCloseStatus.NormalClosure)
                    {
                        DebugConsole.SendMessage(msg);
                    }
                    else
                    {
                        DebugConsole.ThrowError(msg);
                    }

                    ChangeToPreviousOr<MainMenu>();
                    await this.client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    Packet packet = PacketSerializer.Deserialize(buffer);
                    switch (packet.Type)
                    {
                        case PacketType.Ping:
                            var pong = new EmptyPayload() { Type = PacketType.Pong };
                            await this.client.SendAsync(PacketSerializer.ToByteArray(pong), WebSocketMessageType.Text, true, CancellationToken.None);
                            break;

                        case PacketType.GameState:
                            var gameState = packet.GetPayload<GameStatePayload>();
                            this.grid.Logic.UpdateFromStatePayload(gameState.GridState);
                            this.UpdatePlayers(gameState.Players);
                            this.UpdatePlayerBars();
                            this.grid.IsEnabled = true;
                            break;

                        case PacketType.GameData:
                            var gameData = packet.GetPayload<GameDataPayload>();
                            DebugConsole.SendMessage("Broadcast interval: " + gameData.BroadcastInterval + "ms", Color.DarkGray);
                            DebugConsole.SendMessage("Player ID: " + gameData.PlayerId, Color.DarkGray);
                            DebugConsole.SendMessage("Seed: " + gameData.Seed, Color.DarkGray);
                            ServerBroadcastInterval = gameData.BroadcastInterval;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                DebugConsole.ThrowError("An error occurred while receiving messages: " + e.Message);
                DebugConsole.SendMessage("MessageType: " + result?.MessageType, Color.Orange);

                if (result?.MessageType == WebSocketMessageType.Text)
                {
                    DebugConsole.SendMessage("Message: " + PacketSerializer.FromByteArray(buffer));
                }
            }
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
