using System;
using System.Collections.Generic;
using System.Reflection;
using Fastenshtein;
using GameClient.DebugConsoleItems;
using Microsoft.Xna.Framework;

namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents parser for debug console commands.
/// </summary>
internal static class CommandParser
{
    private static readonly int CommandThreshold = 3;

    /// <summary>
    /// Parses user input, identifies and executes the corresponding command or displays appropriate error messages based on text input.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    public static void Parse(object? sender, MonoRivUI.TextInputEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Value))
        {
            return;
        }

        DebugConsole.SendMessage(">" + e.Value.Trim(), Color.MediumPurple, spaceAfterTime: false);

        ICommand? command = GetCommand(e.Value, out int threshold);

        if (command is null || threshold > CommandThreshold)
        {
            DebugConsole.SendMessage("Command not found.", Color.IndianRed);
            return;
        }

        if (threshold == 0 && command is CommandGroupAttribute cg)
        {
            DebugConsole.SendMessage($"'{(cg as ICommand).FullName}' is a command group.", Color.Orange);
            return;
        }

        if (threshold == 0 && command is CommandAttribute c)
        {
            var args = GetArgsFromInput(c, e.Value);
            var parameters = c.Action.Method.GetParameters();

            var convertedArguments = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    convertedArguments[i] = ConvertArgument(args[i], parameters[i].ParameterType);
                }
                catch (ArgumentException)
                {
                    DebugConsole.SendMessage($"Invalid argument '{args[i]}'.", Color.IndianRed);
                    return;
                }
            }

            try
            {
                List<object?> invokeParameters = [];
                int convertedArgumentsIndex = 0;
                foreach (var parameter in parameters)
                {
                    if (parameter.HasDefaultValue)
                    {
                        invokeParameters.Add(parameter.DefaultValue);
                    }
                    else
                    {
                        invokeParameters.Add(convertedArguments[convertedArgumentsIndex++]);
                    }
                }

                _ = c.Action.DynamicInvoke(invokeParameters.ToArray());
            }
            catch (Exception ex) when (
                ex is TargetParameterCountException
                || ex is System.IndexOutOfRangeException)
            {
                DebugConsole.SendMessage($"Invalid number of arguments. (expected: {parameters.Length}, got: {args.Length})", Color.IndianRed);
            }

            return;
        }

        DebugConsole.SendMessage($"Did you mean '{command.FullName}'?", Color.Orange);
    }

    private static ICommand? GetCommand(string rawInput, out int threshold)
    {
        var segments = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        CommandGroupAttribute? parent = null;
        threshold = int.MaxValue;
        foreach (var segment in segments)
        {
            ICommand? result = MatchCommand(parent, segment, out threshold);

            if (result == null || threshold > CommandThreshold)
            {
                return result;
            }

            if (result is CommandGroupAttribute newParent)
            {
                parent = newParent;
            }
            else
            {
                return result;
            }
        }

        return parent;
    }

    private static ICommand? MatchCommand(CommandGroupAttribute? parent, string segment, out int threshold)
    {
        List<ICommand> commands = GetCommandsByGroup(parent);

        int minimumDistance = int.MaxValue;
        ICommand? result = null;
        foreach (var command in commands)
        {
            var comparableCommandName = command.DisplayName;
            var comparableSegment = segment;

            if (!command.CaseSensitive)
            {
                comparableCommandName = comparableCommandName.ToLower();
                comparableSegment = comparableSegment.ToLower();
            }

            int levensteinDistance = Levenshtein.Distance(comparableCommandName, comparableSegment);

            if (levensteinDistance < minimumDistance)
            {
                minimumDistance = levensteinDistance;
                result = command;
            }
        }

        threshold = minimumDistance;
        return result;
    }

    private static List<ICommand> GetCommandsByGroup(CommandGroupAttribute? group)
    {
        List<ICommand> result = new();
        foreach (ICommand command in CommandInitializer.GetCommands())
        {
            if (command.Group == group)
            {
                result.Add(command);
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
}
