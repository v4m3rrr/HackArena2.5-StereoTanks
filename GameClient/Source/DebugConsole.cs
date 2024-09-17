using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Fastenshtein;
using GameClient.DebugConsoleItems;
using GameClient.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents a debug console scene.
/// </summary>
[NoAutoInitialize]
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
    public static void ThrowError(Exception exception)
    {
        ThrowError(exception.Message);
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
                Type = TransformType.Absolute,
                Location = new Point(50, 50),
            },
        };

        this.baseFrame.Transform.Recalculated += (s, e) =>
        {
            var transform = (Transform)s!;
            transform.Size = new Point(800, 450).Clamp(new Point(540, 300), ScreenController.CurrentSize - new Point(100, 100));
        };

        this.font = new ScalableFont("Content\\Fonts\\Consolas.ttf", 7);

        // Background
        _ = new SolidColor(Color.Black * 0.9f) { Parent = this.baseFrame.InnerContainer };

        // Info text
        {
            var text = new Text(this.font, Color.CornflowerBlue)
            {
                Parent = this.baseFrame.InnerContainer,
                Value = "DEBUG CONSOLE",
                TextAlignment = Alignment.Left,
                TextShrink = TextShrinkMode.HeightAndWidth,
                Spacing = 5,
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

            var text = new Text(this.font, Color.White)
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
                Transform = { Alignment = Alignment.Left },
                TextAlignment = Alignment.Left,
                TextShrink = TextShrinkMode.SafeCharHeight,
                Placeholder = "Enter command...",
                PlaceholderOpacity = 0.5f,
                ClearAfterSend = true,
                DeselectAfterSend = false,
            };
            this.Showed += (s, e) => this.textInput.Select();
            this.Hid += (s, e) => this.textInput.Deselect();
            this.textInput.TextInputSent += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Value))
                {
                    return;
                }

                SendMessage(">" + e.Value.Trim(), Color.MediumPurple, spaceAfterTime: false);

                ICommand? command = GetCommandFromInput(e.Value, out int threshold);

                if (command is null || threshold > 3)
                {
                    SendMessage("Command not found.", Color.IndianRed);
                }
                else if (threshold == 0 && command is CommandGroupAttribute cg)
                {
                    SendMessage($"'{(cg as ICommand).FullName}' is a command group.", Color.Orange);
                }
                else if (threshold == 0 && command is CommandAttribute c)
                {
                    var args = GetArgsFromInput(c, e.Value);
                    var parameters = c.Action.Method.GetParameters();

                    var areArgumentsValid = true;

                    var convertedArgs = new object[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        try
                        {
                            convertedArgs[i] = ConvertArgument(args[i], parameters[i].ParameterType);
                        }
                        catch (ArgumentException)
                        {
                            SendMessage($"Invalid argument '{args[i]}'.", Color.IndianRed);
                            areArgumentsValid = false;
                            break;
                        }
                    }

                    try
                    {
                        if (areArgumentsValid)
                        {
                            // Refactor!!!
                            var convertedArgs2 = new List<object?>();
                            int i = 0;
                            foreach (var parameter in c.Action.Method.GetParameters())
                            {
                                if (parameter.HasDefaultValue)
                                {
                                    convertedArgs2.Add(parameter.DefaultValue);
                                }
                                else
                                {
                                    convertedArgs2.Add(convertedArgs[i++]);
                                }
                            }

                            c.Action.DynamicInvoke(convertedArgs2.ToArray());
                        }
                    }
                    catch (TargetParameterCountException)
                    {
                        SendMessage($"Invalid number of arguments. (expected: {parameters.Length}, got: {args.Length})", Color.IndianRed);
                    }
                }
                else
                {
                    SendMessage($"Did you mean '{command.FullName}'?", Color.Orange);
                }
            };
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
                Spacing = 8,
                Transform =
                {
                    RelativePadding = new Vector4(0.005f, 0.02f, 0.005f + 0.02f, 0.02f),
                },
                DrawContentOnParentPadding = true,
                ShowScrollBarIfNotNeeded = false,
            };
        }

#if DEBUG
        SendMessage("You are running in the DEBUG mode.", Color.Yellow);
#endif

        SendMessage("Type 'help' to get list of available commands.", Color.White);
    }

    private static ICommand? GetCommandFromInput(string input, out int threshold)
    {
        threshold = int.MaxValue;

        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        ICommand? foundCommand = null;
        var lev = new Levenshtein(input);
        string[] inputParts = Regex.Split(input, @"\s+");
        int maxDepth = inputParts.Length - 1;
        foreach (ICommand command in GetAllCommandCombinations(maxDepth))
        {
            int levenshteinDistance = int.MaxValue;
            if (command is CommandAttribute c && c.Arguments.Length <= maxDepth)
            {
                for (int i = 0; i < c.Arguments.Length; i++)
                {
                    levenshteinDistance = Math.Min(levenshteinDistance, Levenshtein.Distance(
                        string.Join(' ', inputParts[..^(i + 1)]), command.FullName));
                }
            }

            var fullName = command.CaseSensitive ? command.FullName : command.FullName.ToLower();
            levenshteinDistance = Math.Min(levenshteinDistance, lev.DistanceFrom(fullName));

            if (threshold > levenshteinDistance)
            {
                threshold = levenshteinDistance;
                foundCommand = command;
            }
        }

        return foundCommand;
    }

    private static List<ICommand> GetAllCommandCombinations(int maxDepth, CommandGroupAttribute? group = null)
    {
        var result = new List<ICommand>();
        foreach (ICommand command in CommandInitializer.GetCommands())
        {
            result.Add(command);
            if (command is CommandGroupAttribute commandGroup && commandGroup == group && command.Depth < maxDepth)
            {
                result.AddRange(GetAllCommandCombinations(maxDepth, commandGroup));
            }
        }

        return result;
    }

    private static string[] GetArgsFromInput(ICommand command, string input)
    {
        return input[command.FullName.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static object ConvertArgument(string argument, Type targetType)
    {
        return targetType.IsEnum
            ? Enum.Parse(targetType, argument, ignoreCase: true)
            : Convert.ChangeType(argument, targetType);
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
