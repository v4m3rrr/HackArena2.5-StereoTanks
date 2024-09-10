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
    private readonly List<PlayerStatsBar> playerStatsBars = [];
    private readonly List<PlayerIdentityBar> playerIdentityBars = [];

    private readonly GridComponent grid;

    private readonly ListBox playerIdentityBox;
    private readonly ListBox playerStatsBox;

    private ClientWebSocket client;
    private string? playerId = null;
    private bool isSpectator;

    private Text matchName = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Game"/> class.
    /// </summary>
    public Game()
        : base(Color.Transparent)
    {
        this.client = new ClientWebSocket();

        this.grid = new GridComponent()
        {
            IsEnabled = false,
            Parent = this.BaseComponent,
            Transform =
            {
                Alignment = Alignment.Center,
                Ratio = new Ratio(1, 1),
                RelativeSize = new Vector2(0.8f),
                RelativeOffset = new Vector2(0.0f, 0.03f),
            },
        };

        this.playerIdentityBox = new AlignedListBox()
        {
            Parent = this.BaseComponent,
            ElementsAlignment = Alignment.Center,
            Spacing = 8,
            Transform =
            {
                Alignment = Alignment.Left,
                RelativeSize = new Vector2(0.23f, 1.0f),
                RelativePadding = new Vector4(0.05f),
            },
        };

        this.playerStatsBox = new AlignedListBox()
        {
            Parent = this.BaseComponent,
            ElementsAlignment = Alignment.Center,
            Spacing = 5,
            Transform =
            {
                Alignment = Alignment.Right,
                RelativeSize = new Vector2(0.23f, 1.0f),
                RelativePadding = new Vector4(0.05f),
            },
        };
    }

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
        if (MainMenu.Effect.Rotation != 0.0f)
        {
            int sign = MainMenu.Effect.Rotation is > MathHelper.Pi or < 0 and > -MathHelper.Pi ? 1 : -1;
            var value = 0.25f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MainMenu.Effect.Rotation += Math.Min(MainMenu.Effect.Rotation, Math.Min(value, 0.1f)) * sign;
            MainMenu.Effect.Rotation %= MathHelper.TwoPi;
        }

        this.HandleInput();
        base.Update(gameTime);
    }

    /// <inheritdoc/>
    public override void Draw(GameTime gameTime)
    {
        ScreenController.GraphicsDevice.Clear(Color.Black);
        MainMenu.Effect.Draw(gameTime);

        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showing += this.Game_Showing;
        this.Hiding += this.Game_Hiding;

        var font = new ScalableFont("Content/Fonts/Orbitron-SemiBold.ttf", 15);
        this.matchName = new Text(font, Color.White)
        {
            Parent = baseComponent,
            Value = "Match Name",
            Case = TextCase.Upper,
            AdjustTransformSizeToText = AdjustSizeOption.HeightAndWidth,
            Transform =
            {
                Alignment = Alignment.Top,
                RelativeOffset = new Vector2(0.0f, 0.03f),
            },
        };
    }

    private async void Game_Showing(object? sender, SceneDisplayEventArgs? e)
    {
        if (e is not DisplayEventArgs args)
        {
            DebugConsole.ThrowError(
                $"Game scene requires {nameof(DisplayEventArgs)}.");
            ChangeToPreviousOrDefault<MainMenu>();
            return;
        }

        this.isSpectator = args.IsSpectator;

        await this.ConnectAsync(args.JoinCode);
    }

    private async void Game_Hiding(object? sender, EventArgs e)
    {
        this.grid.ResetFogOfWar();
        this.grid.IsEnabled = false;

        if (this.client.State == WebSocketState.Open)
        {
            await this.client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }

    private async Task ConnectAsync(string? joinCode)
    {
        string server = $"ws://{GameSettings.ServerAddress}:{GameSettings.ServerPort}"
            + $"/{(this.isSpectator ? "spectator" : string.Empty)}";

        if (joinCode is not null)
        {
            server += $"?joinCode={joinCode}";
        }

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
                ChangeToPreviousOrDefault<MainMenu>();
                return;
            }
            catch (Exception ex)
            {
                DebugConsole.ThrowError($"An error occurred while sending HTTP request: {ex.Message}");
                ChangeToPreviousOrDefault<MainMenu>();
                return;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.TooManyRequests)
            {
                string errorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                DebugConsole.ThrowError($"Server error: {errorMessage}");
                ChangeToPreviousOrDefault<MainMenu>();
                return;
            }
            else if (response.StatusCode != HttpStatusCode.OK)
            {
                DebugConsole.ThrowError($"Unexpected response from server: {response.StatusCode}");
                ChangeToPreviousOrDefault<MainMenu>();
                return;
            }
        }

        this.client = new ClientWebSocket();

        try
        {
            await this.client.ConnectAsync(new Uri(server), CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            DebugConsole.ThrowError($"An error occurred while connecting to the server: {ex.Message}");
            ChangeToPreviousOrDefault<MainMenu>();
            return;
        }

        _ = Task.Run(this.ReceiveMessages);
        DebugConsole.SendMessage("Server status: connected", Color.LightGreen);
    }

    // What matters is that it works
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

                    ChangeToPreviousOrDefault<MainMenu>();
                    await this.client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    Packet packet = PacketSerializer.Deserialize(buffer);
                    try
                    {
                        switch (packet.Type)
                        {
                            case PacketType.Ping:
                                var pong = new EmptyPayload() { Type = PacketType.Pong };
                                await this.client.SendAsync(PacketSerializer.ToByteArray(pong), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;

                            case PacketType.GameState:

                                GameStatePayload gameState = null!;

                                SerializationContext context = this.isSpectator
                                    ? new SerializationContext.Spectator()
                                    : new SerializationContext.Player(this.playerId!);

                                var converters = GameStatePayload.GetConverters(context);
                                var serializer = PacketSerializer.GetSerializer(converters);

                                try
                                {
                                    gameState = this.isSpectator
                                        ? packet.GetPayload<GameStatePayload>(serializer)
                                        : packet.GetPayload<GameStatePayload.ForPlayer>(serializer);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                    break;
                                }

                                this.grid.Logic.UpdateFromStatePayload(gameState);
                                this.UpdatePlayers(gameState.Players);
                                this.UpdatePlayerBars();

                                if (gameState is GameStatePayload.ForPlayer playerGameState)
                                {
                                    var player = this.players[playerGameState.PlayerId];
                                    this.grid.UpdateFogOfWar(playerGameState.VisibilityGrid, new Color(player.Color));
                                }

                                this.grid.IsEnabled = true;
                                break;

                            case PacketType.GameData:
                                var gameData = packet.GetPayload<GameDataPayload>();
                                DebugConsole.SendMessage("Broadcast interval: " + gameData.BroadcastInterval + "ms", Color.DarkGray);
                                DebugConsole.SendMessage("Player ID: " + gameData.PlayerId, Color.DarkGray);
                                this.playerId = gameData.PlayerId;
                                DebugConsole.SendMessage("Seed: " + gameData.Seed, Color.DarkGray);
                                ServerBroadcastInterval = gameData.BroadcastInterval;
                                this.grid.IsEnabled = true;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        DebugConsole.ThrowError($"An error occurred while processing the packet {packet.Type}: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                if (this.client.State == WebSocketState.Closed)
                {
                    // Ignore
                    break;
                }

                DebugConsole.ThrowError($"An error occurred while receiving messages: " + e.Message);
                DebugConsole.SendMessage("MessageType: " + result?.MessageType, Color.Orange);
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

    private void UpdatePlayers(IEnumerable<Player> updatedPlayers)
    {
        foreach (Player updatedPlayer in updatedPlayers)
        {
            if (this.players.TryGetValue(updatedPlayer.Id, out var existingPlayer))
            {
                existingPlayer.UpdateFrom(updatedPlayer);
            }
            else
            {
                this.players[updatedPlayer.Id] = updatedPlayer;
            }
        }

        this.players
            .Where(x => !updatedPlayers.Contains(x.Value))
            .ToList()
            .ForEach(x => this.players.Remove(x.Key));
    }

    private void UpdatePlayerBars()
    {
        var newPlayerBars = this.players.Values
            .Where(player => this.playerIdentityBars.All(pb => pb.Player != player) && player is not null)
            .Select(player => new PlayerIdentityBar(player)
            {
                Parent = this.playerIdentityBox.ContentContainer,
                Transform =
                {
                    RelativeSize = new Vector2(1f, 0.2f),
                    Alignment = Alignment.Top,
                    MaxSize = new Point(310, 110),
                },
            })
            .ToList();

        this.playerIdentityBars.AddRange(newPlayerBars);

        foreach (PlayerIdentityBar playerBar in this.playerIdentityBars.ToList())
        {
            if (!this.players.ContainsValue(playerBar.Player))
            {
                playerBar.Parent = null;
                _ = this.playerIdentityBars.Remove(playerBar);
            }
        }

        var newPlayerBars2 = this.players.Values
            .Where(player => this.playerStatsBars.All(pb => pb.Player != player) && player is not null)
            .Select(player => new PlayerStatsBar(player)
            {
                Parent = this.playerStatsBox.ContentContainer,
                Transform =
                {
                    RelativeSize = new Vector2(1f, 0.2f),
                    Alignment = Alignment.Top,
                    MaxSize = new Point(310, 110),
                },
            })
            .ToList();

        this.playerStatsBars.AddRange(newPlayerBars2);

        foreach (PlayerStatsBar playerBar in this.playerStatsBars.ToList())
        {
            if (!this.players.ContainsValue(playerBar.Player))
            {
                playerBar.Parent = null;
                _ = this.playerStatsBars.Remove(playerBar);
            }
        }
    }

    /// <summary>
    /// Represents the event arguments for the <see cref="Game"/> scene.
    /// </summary>
    /// <param name="joinCode">The join code to join the game.</param>
    /// <param name="isSpectator">A value indicating whether the player is a spectator.</param>
    public class DisplayEventArgs(string? joinCode, bool isSpectator) : SceneDisplayEventArgs
    {
        /// <summary>
        /// Gets the join code to join the game.
        /// </summary>
        public string? JoinCode { get; } = joinCode;

        /// <summary>
        /// Gets a value indicating whether the player is a spectator.
        /// </summary>
        public bool IsSpectator { get; } = isSpectator;
    }
}
