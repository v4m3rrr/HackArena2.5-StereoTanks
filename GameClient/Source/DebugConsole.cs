using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GameClient.DebugConsoleItems;
using GameClient.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents a debug console scene.
/// </summary>
internal partial class DebugConsole : Scene, IOverlayScene
{
    private Frame baseFrame = default!;
    private ScalableFont font = default!;

    private TextInput textInput = default!;
    private ListBox messages = default!;

    private bool openedInThisFrame;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugConsole"/> class.
    /// </summary>
    public DebugConsole()
        : base()
    {
        Instance = this;
        CommandInitializer.Initialize();
    }

    /// <summary>
    /// Gets the instance of the debug console.
    /// </summary>
    public static DebugConsole Instance { get; private set; } = default!;

    /// <inheritdoc/>
    public int Priority => int.MaxValue;

    /// <inheritdoc/>
    public IEnumerable<IComponent> OverlayComponents => [this.baseFrame];

    /// <summary>
    /// Clears all messages from the debug console.
    /// </summary>
    public static void Clear()
    {
        Instance.messages.Clear();
    }

    /// <summary>
    /// Sends a message to the debug console.
    /// </summary>
    /// <param name="text">The text to be sent.</param>
    /// <param name="color">The color of the message text.</param>
    /// <param name="spaceAfterTime">Whether to add a space after the time.</param>
    public static void SendMessage(string text, Color? color = null, bool spaceAfterTime = true)
    {
        var sb = new StringBuilder()
            .Append('[')
            .Append(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
            .Append(']')
            .Append(spaceAfterTime ? ' ' : char.MinValue)
            .Append(text.Trim());

        _ = new WrappedText(Instance.font, color ?? Color.White)
        {
            Parent = Instance.messages.ContentContainer,
            Value = sb.ToString(),
            AdjustTransformSizeToText = AdjustSizeOption.OnlyHeight,
        };
    }

    /// <summary>
    /// Throws an error message to the debug console.
    /// </summary>
    /// <param name="message">The error message.</param>
    public static void ThrowError(string message)
    {
        SendMessage(message, Color.Red);

#if DEBUG
        if (!Instance.IsDisplayedOverlay)
        {
            ShowOverlay<DebugConsole>(default);
            Instance.textInput.Deselect();
        }
#endif
    }

    /// <summary>
    /// Throws an error message to the debug console.
    /// </summary>
    /// <param name="exception">The exception to be thrown.</param>
    /// <param name="withTraceback">Whether to print the traceback, if present.</param>
#if DEBUG
    public static void ThrowError(Exception exception, bool withTraceback = true)
#else
    public static void ThrowError(Exception exception, bool withTraceback = false)
#endif
    {
        ThrowError(exception.Message);

        if (withTraceback && exception.StackTrace is not null)
        {
            SendMessage(exception.StackTrace, Color.Red.WithAlpha(200));
        }
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (KeyboardController.IsKeyHit(Keys.OemTilde)
            && KeyboardController.IsKeyDown(Keys.LeftControl))
        {
            if (this.openedInThisFrame)
            {
                this.openedInThisFrame = false;
            }
            else
            {
                this.Close();
                return;
            }
        }

        if (KeyboardController.IsKeyHit(Keys.Tab))
        {
            Autocomplete();
        }

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    protected override void Initialize(Component baseComponent)
    {
        this.Showed += (s, e) => this.openedInThisFrame = true;
        this.Showed += (s, e) =>
        {
            if (this.IsDisplayedOverlay)
            {
                Current.BaseComponent.GetAllDescendants<TextInput>().ToList().ForEach(x => x.Deselect());
            }
        };

        this.baseFrame = new Frame(Color.Black, 2)
        {
            Parent = baseComponent,
            Transform =
            {
                RelativeSize = new Vector2(0.46f),
                Alignment = Alignment.TopLeft,
                RelativeOffset = new Vector2(0.02f, 0.035f),
                MinSize = new Point(540, 300),
            },
        };

        this.font = new ScalableFont("Content\\Fonts\\Consolas.ttf", 8)
        {
            MinSize = 6,
            AutoResize = true,
        };

        // Background
        _ = new SolidColor(Color.Black * 0.9f) { Parent = this.baseFrame.InnerContainer };

        // Info text
        {
            var font = new ScalableFont(Styles.Fonts.Paths.Main, 8);

            var text = new Text(font, Color.CornflowerBlue)
            {
                Parent = this.baseFrame.InnerContainer,
                Value = "DEBUG CONSOLE",
                Spacing = 5,
                TextAlignment = Alignment.Left,
                TextShrink = TextShrinkMode.HeightAndWidth,
                Transform =
                {
                    Alignment = Alignment.TopLeft,
                    RelativeOffset = new Vector2(0.005f, 0f),
                    RelativeSize = new Vector2(0.95f, 0.042f),
                },
            };
        }

        // Close buttom
        {
            var button = new Button<SolidColor>(new SolidColor(Color.DarkRed))
            {
                Parent = this.baseFrame.InnerContainer,
                Transform =
                {
                    Alignment = Alignment.TopRight,
                    RelativeSize = new Vector2(0.04f),
                    RelativeOffset = new Vector2(-0.003f, 0.005f),
                    Ratio = new Ratio(1, 1),
                },
            };

            var font = new ScalableFont(Styles.Fonts.Paths.Main, 8);

            var text = new Text(font, Color.White)
            {
                Parent = button,
                Value = "X",
                TextAlignment = Alignment.Center,
                TextShrink = TextShrinkMode.HeightAndWidth,
                Scale = 0.9f,
                Transform =
                {
                    RelativeOffset = new Vector2(-0.05f, -0.06f),
                },
            };

            button.Clicked += (s, e) => this.Close();
            button.HoverEntered += (s, e) => e.Color = Color.Red;
            button.HoverExited += (s, e) => e.Color = Color.DarkRed;
        }

        // Text input
        {
            var frame = new Frame(new Color(60, 60, 60, 255), thickness: 2)
            {
                Parent = this.baseFrame.InnerContainer,
                Transform =
                {
                    Alignment = Alignment.Bottom,
                    RelativeSize = new Vector2(0.995f, 0.05f),
                    RelativeOffset = new Vector2(0.0f, -0.01f),
                    RelativePadding = new Vector4(0.005f, 0.07f, 0.005f, 0.07f),
                },
            };

            var background = new SolidColor(Color.Gray * 0.5f)
            {
                Parent = frame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };

            this.textInput = new TextInput(this.font, Color.White, caretColor: Color.DarkGray)
            {
                Parent = frame.InnerContainer,
                Transform =
                {
                    Alignment = Alignment.Left,
                    RelativePadding = new Vector4(0.001f, 0.01f, 0.001f, 0.01f),
                },
                TextAlignment = Alignment.Left,
                TextShrink = TextShrinkMode.SafeCharHeight,
                Placeholder = "Enter command...",
                PlaceholderOpacity = 0.5f,
                ClearAfterSend = true,
                DeselectAfterSend = false,
            };
            this.Showed += (s, e) => this.textInput.Select();
            this.Hid += (s, e) => this.textInput.Deselect();
            this.textInput.TextInputSent += CommandParser.Parse;
        }

        // Messages
        {
            var messagesFrame = new Frame(new Color(60, 60, 60, 255), thickness: 2)
            {
                Parent = this.baseFrame.InnerContainer,
                Transform =
                {
                    Alignment = Alignment.Top,
                    RelativeSize = new Vector2(0.995f, 0.885f),
                    RelativeOffset = new Vector2(0.0f, 0.05f),
                },
            };

            var background = new SolidColor(Color.Gray * 0.15f)
            {
                Parent = messagesFrame.InnerContainer,
                Transform = { IgnoreParentPadding = true },
            };

            var messages = this.messages = new ScrollableListBox(new SolidColor(Color.Red))
            {
                Parent = messagesFrame.InnerContainer,
                Orientation = Orientation.Vertical,
                MaxElements = 300,
                Spacing = 8,
                Transform =
                {
                    RelativePadding = new Vector4(0.005f, 0.02f, 0.005f + 0.02f, 0.02f),
                },
                DrawContentOnParentPadding = true,
                ShowScrollBarIfNotNeeded = false,
            };

            float scrollPosition = 0f;
            messages.ComponentAdding += (s, e) => scrollPosition = ((ScrollableListBox)Instance.messages).ScrollBar.Position;
            messages.ComponentAdded += (s, e) =>
            {
                if (scrollPosition == 1.0f)
                {
                    messages.ForceUpdate();
                    ScrollToBottom();
                }
            };
            ((ScrollableListBox)Instance.messages).ScrollBar.Enabled += (s, e) => ScrollToBottom();
        }
#if DEBUG
        SendMessage("You are running in the DEBUG mode.", Color.Yellow);
#endif

        SendMessage("Type 'help' to get list of available commands.", Color.White);
    }

    private static void ScrollToBottom()
    {
        ((ScrollableListBox)Instance.messages).ScrollBar.ScrollTo(1.0f);
    }

    private static void Autocomplete()
    {
        var incompleteCommand = Instance.textInput.Value;

        if (incompleteCommand.Length <= 4)
        {
            return;
        }

        var bestMatch = CommandParser.GetCommand(incompleteCommand, out int threshold);

        if (bestMatch == null)
        {
            return;
        }

        if (bestMatch.FullName.Length < incompleteCommand.Length)
        {
            return;
        }

        Instance.textInput.SetText(bestMatch.FullName);
    }

    /// <inheritdoc/>
    protected override void LoadSceneContent()
    {
        var textures = this.BaseComponent.GetAllDescendants<TextureComponent>();
        textures.ToList().ForEach(x => x.Load());
    }

    private void Close()
    {
        if (this.IsDisplayedOverlay)
        {
            HideOverlay<DebugConsole>();
        }
        else
        {
            ChangeToPreviousOrDefaultWithoutStack<MainMenu>();
        }
    }
}
