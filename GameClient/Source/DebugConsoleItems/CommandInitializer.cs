using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GameClient.Networking;
using Microsoft.Xna.Framework;
using MonoRivUI;

#if DEBUG

using System.Threading;
using GameClient.Scenes;
using GameClient.UI;
using GameLogic.Networking;

#endif

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
    private static async void Exit()
    {
        if (ServerConnection.IsConnected)
        {
            await ServerConnection.CloseAsync("Exit the game.");
        }

        GameClientCore.Instance.Exit();
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
        private static async void Set(
            [Argument("A new width of the screen.")] int width,
            [Argument("A new height of the screen.")] int height)
        {
            if (width < GameClientCore.MinWindowSize.X || height < GameClientCore.MinWindowSize.Y)
            {
                DebugConsole.SendMessage($"The minimum resolution must be {GameClientCore.MinWindowSize.X}x{GameClientCore.MinWindowSize.Y}.", Color.IndianRed);
                return;
            }

            await GameSettings.SetResolution(width, height);
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
        private static async void Set([Argument("A new screen type.")] ScreenType screenType)
        {
            await GameSettings.SetScreenType(screenType);
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
            int languageMemberCount = Enum.GetNames(typeof(Language)).Length;
            var languageValue = (int)language;
            if (languageValue < 0 || languageValue >= languageMemberCount)
            {
                DebugConsole.SendMessage($"Invalid argument {language}", Color.IndianRed);
                return;
            }

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

    [Command("Crash the game.")]
    private static void Crash()
    {
        throw new CrashIntentionallyException();
    }

#if DEBUG
    [Command("Show current scene stack.")]
    private static void SceneStack()
    {
        var stack = (Stack<(Scene, SceneDisplayEventArgs)>)typeof(Scene)
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
        var overlays = ScreenController.DisplayedOverlays;
        var sb = new StringBuilder();

        foreach (var overlay in overlays)
        {
            if (sb.Length > 0)
            {
                _ = sb.Append(" -> ");
            }

            _ = sb.Append(overlay.Value.GetType().Name)
                .Append(" (")
                .Append(overlay.Value.Priority)
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
    private static async void MainMenu()
    {
        if (ServerConnection.IsConnected)
        {
            await ServerConnection.CloseAsync("Leave the game.");
        }

        Scene.ChangeWithoutStack<MainMenu>();
        Scene.ResetSceneStack();
        DebugConsole.SendMessage("Changed to main menu scene.", Color.Green);
    }

    [Command("Change to previous scene.")]
    private static async void PreviousScene()
    {
        try
        {
            if (ServerConnection.IsConnected)
            {
                await ServerConnection.CloseAsync("Leave the game.");
            }

            Scene.ChangeToPrevious();
        }
        catch (InvalidOperationException ex)
        {
            DebugConsole.SendMessage(ex.Message, Color.IndianRed);
        }
    }

    [CommandGroup(Name = "Game", Description = "Interact with the game.")]
    private static class GameCommand
    {
        private static bool ThrowErrorIfNotGameScene()
        {
            if (Scene.Current is not Scenes.Game game)
            {
                DebugConsole.ThrowError("The current scene is not a game scene.");
                return true;
            }

            return false;
        }

        private static bool ThrowErrorIfNotConnectedToServer()
        {
            if (!ServerConnection.IsConnected)
            {
                DebugConsole.ThrowError("The server is not connected.");
                return true;
            }

            return false;
        }

#if STEREO
        [Command("Sets a score to a team.")]
        private static async void SetScore(
            [Argument("The name of the team whose score is to be set.")] string name,
            [Argument("The number of points to assign to the specified team.")] int points)
#else
        [Command("Sets a score to a player.")]
        private static async void SetScore(
            [Argument("The nickname of the player whose score is to be set.")] string name,
            [Argument("The number of points to assign to the specified player.")] int points)
#endif
        {
            if (ThrowErrorIfNotGameScene() || ThrowErrorIfNotConnectedToServer())
            {
                return;
            }

            var payload = new SetScorePayload(name, points);
            var message = PacketSerializer.Serialize(payload);

            await ServerConnection.SendAsync(message);

#if STEREO
            DebugConsole.SendMessage(
                $"Packet \"Set team '{name}' points to {points}\" has been sent to the server.",
#else
            DebugConsole.SendMessage(
                $"Packet \"Set player '{name}' points to {points}\" has been sent to the server.",
#endif
                Color.Green);
        }

        [Command("Force end the game.")]
        private static async void ForceEnd()
        {
            if (ThrowErrorIfNotGameScene() || ThrowErrorIfNotConnectedToServer())
            {
                return;
            }

            var payload = new EmptyPayload() { Type = PacketType.ForceEndGame };
            var message = PacketSerializer.Serialize(payload);

            await ServerConnection.SendAsync(message);

            DebugConsole.SendMessage(
                $"Packet \"Force end the game\" has been sent to the server.",
                Color.Green);
        }
    }

#endif
}
