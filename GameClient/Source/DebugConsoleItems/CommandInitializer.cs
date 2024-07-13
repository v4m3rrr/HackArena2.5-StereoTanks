using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents a command initializer.
/// </summary>
internal static class CommandInitializer
{
    private static readonly List<ICommand> Commands = new();
    private static bool isInitialzed;

    /// <summary>
    /// Initializes the debug console commands.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the debug console commands have already been initialized.
    /// </exception>
    public static void Initialize()
    {
        if (isInitialzed)
        {
            throw new InvalidOperationException(
                "CommandInitializer has already been initialized.");
        }

        LoadCommandsFromType(typeof(CommandInitializer), null);
        isInitialzed = true;
    }

    /// <summary>
    /// Gets the commands.
    /// </summary>
    /// <returns>The commands.</returns>
    public static IEnumerable<ICommand> GetCommands()
    {
        return Commands;
    }

    private static void LoadCommandsFromType(Type type, CommandGroupAttribute? parentGroup)
    {
        foreach (Type nestedType in type.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (nestedType.GetCustomAttribute<CommandGroupAttribute>() is { } group)
            {
                group.Initialize(nestedType, parentGroup);
                Commands.Add(group);

                LoadCommandsFromType(nestedType, group);
            }
        }

        MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
        foreach (MethodInfo method in methods)
        {
            if (method.GetCustomAttribute<CommandAttribute>() is { } command)
            {
                command.Initialize(method, parentGroup);
                Commands.Add(command);
            }
        }
    }

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable SA1201 // Elements should appear in the correct order

    [Command("Exit the game.")]
    private static void Exit()
    {
        MonoTanks.Instance.Exit();
    }

    [Command("Clear the debug console.")]
    private static void Clear()
    {
        DebugConsole.Clear();
    }

    [Command("Display the list of available commands.")]
    private static void Help()
    {
        var message = new StringBuilder()
            .AppendLine("Available commands:");

        var maxCommandLength = Commands.Max(x => x.FullName.Length);

        foreach (var command in Commands.Where(x => x is CommandAttribute).OrderBy(x => x.FullName))
        {
            var commandLength = command.FullName.Length;
            var padding = new string(' ', maxCommandLength - commandLength);

            _ = message.Append("  ")
                .Append(command.CaseSensitive ? command.FullName : command.FullName.ToLower())
                .Append(padding)
                .Append(" -> ")
                .AppendLine(command.Description);
        }

        DebugConsole.SendMessage(message.ToString());
    }

    [CommandGroup("Interact with the game window resolution.")]
    private static class Resolution
    {
        [Command("Set a new window resolution.")]
        private static void Set(
            [Argument("A new width of the screen.")] int width,
            [Argument("A new height of the screen.")] int height)
        {
            if (width < MonoTanks.MinWindowSize.X || height < MonoTanks.MinWindowSize.Y)
            {
                DebugConsole.SendMessage($"The minimum resolution must be {MonoTanks.MinWindowSize.X}x{MonoTanks.MinWindowSize.Y}.", Color.IndianRed);
                return;
            }

            GameSettings.SetResolution(width, height);
            GameSettings.SaveSettings();

            var resolution = $"{ScreenController.Width}x{ScreenController.Height}";
            DebugConsole.SendMessage($"Resolution has been set to {resolution}.", Color.Green);
        }

        [Command("Get the current window resolution.")]
        private static void Get()
        {
            DebugConsole.SendMessage($"{ScreenController.Width}x{ScreenController.Height}");
        }
    }

    [CommandGroup(Name = "ScreenType", Description = "Interact with the screen type.")]
    private static class ScreenTypeCommand
    {
        [Command("Set a new screen type.")]
        private static void Set([Argument("A new screen type.")] ScreenType screenType)
        {
            GameSettings.SetScreenType(screenType);
            GameSettings.SaveSettings();
            DebugConsole.SendMessage($"Screen type has been changed to {screenType}.", Color.Green);
        }

        [Command("Get the current screen type.")]
        private static void Get()
        {
            DebugConsole.SendMessage(ScreenController.ScreenType.ToString());
        }
    }

    [CommandGroup(Name = "Language", Description = "Interact with the language.")]
    private static class LanguageCommand
    {
        [Command("Set a new language.")]
        private static void Set([Argument("A new screen type.")] Language language)
        {
            GameSettings.Language = language;
            GameSettings.SaveSettings();
            DebugConsole.SendMessage($"Language has been changed to {language}.", Color.Green);
            if (language != Language.English)
            {
                DebugConsole.SendMessage(
                    $"Please note that the debug console will always be in English.",
                    Color.Yellow);
            }
        }

        [Command("Get the current language.")]
        private static void Get()
        {
            DebugConsole.SendMessage(GameSettings.Language.ToString());
        }
    }

#if DEBUG
    [Command("Show current scene stack.")]
    private static void SceneStack()
    {
        var stack = (Stack<Scene>)typeof(Scene)
            .GetField("SceneStack", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        var sb = new StringBuilder();

        foreach (var scene in stack)
        {
            if (sb.Length > 0)
            {
                _ = sb.Append(" -> ");
            }

            _ = sb.Append(scene.GetType().Name);
        }

        DebugConsole.SendMessage(sb.Length == 0 ? "-" : sb.ToString());
    }

    [Command("Show current scene overlays with priorities.")]
    private static void Overlays()
    {
        var overlays = Scene.DisplayedOverlays;
        var sb = new StringBuilder();

        foreach (var overlay in overlays)
        {
            if (sb.Length > 0)
            {
                _ = sb.Append(" -> ");
            }

            _ = sb.Append(overlay.Scene.GetType().Name)
                .Append(" (")
                .Append(overlay.Scene.Priority)
                .Append(")");
        }

        DebugConsole.SendMessage(sb.Length == 0 ? "-" : sb.ToString());
    }

    [Command("Show current scene name.")]
    private static void CurrentScene()
    {
        DebugConsole.SendMessage(Scene.Current.GetType().Name);
    }

    [Command("Change to main menu scene.")]
    private static void MainMenu()
    {
        Scene.Change<Scenes.MainMenu>(addCurrentToStack: false);
        Scene.ResetSceneStack();
        DebugConsole.SendMessage("Changed to main menu scene.", Color.Green);
    }

    [Command("Change to previous scene.")]
    private static void PreviousScene()
    {
        try
        {
            Scene.ChangeToPrevious();
        }
        catch (InvalidOperationException ex)
        {
            DebugConsole.SendMessage(ex.Message, Color.IndianRed);
        }
    }
#endif
}