using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Fastenshtein;
using Microsoft.Xna.Framework;

namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents parser for debug console commands.
/// </summary>
internal static class CommandParser
{
    private static readonly int CommandThreshold = 3;
    private static readonly int ExactMatchThreshold = 0;

    /// <summary>
    /// Parses user input, identifies and executes the corresponding command
    /// or displays appropriate error messages based on text input.
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

        if (threshold == ExactMatchThreshold && command is CommandGroupAttribute cg)
        {
            DebugConsole.SendMessage($"'{(cg as ICommand).FullName}' is a command group.", Color.Orange);
            return;
        }

        if (threshold == ExactMatchThreshold && command is CommandAttribute c)
        {
            var args = GetArgsFromInput(c, e.Value);
            var parameters = c.Action.Method.GetParameters();

            var convertedArguments = new object[args.Length];

            if (args.Length > parameters.Length)
            {
                DebugConsole.SendMessage($"Invalid number of arguments. (expected: {parameters.Length}, got: {args.Length})", Color.IndianRed);
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    convertedArguments[i] = ConvertArgument(args[i], parameters[i].ParameterType);
                }
                catch (Exception)
                {
                    DebugConsole.SendMessage($"Invalid argument '{args[i]}'.", Color.IndianRed);
                    return;
                }
            }

            List<object?> invokeParameters = [];
            try
            {
                int convertedArgumentsIndex = 0;
                foreach (var parameter in parameters)
                {
                    if (parameter.HasDefaultValue && convertedArgumentsIndex >= convertedArguments.Length)
                    {
                        invokeParameters.Add(parameter.DefaultValue);
                    }
                    else
                    {
                        invokeParameters.Add(convertedArguments[convertedArgumentsIndex++]);
                    }
                }
            }
            catch (Exception ex) when (
                ex is TargetParameterCountException
                or System.IndexOutOfRangeException)
            {
                DebugConsole.SendMessage($"Invalid number of arguments. (expected: {parameters.Length}, got: {args.Length})", Color.IndianRed);
                return;
            }

            try
            {
                _ = c.Action.DynamicInvoke([.. invokeParameters]);
            }
            catch (Exception ex)
            {
                DebugConsole.ThrowError(ex.Message);
            }

            return;
        }

        DebugConsole.SendMessage($"Did you mean '{command.FullName}'?", Color.Orange);
    }

    /// <summary>
    /// Finds command that is closest to given input using Levenshtein distance.
    /// </summary>
    /// <param name="rawInput">Text that will be used for command matching.</param>
    /// <param name="threshold">Levenshtein distance of found match.</param>
    /// <returns>ICommand object based on raw text.</returns>
    public static ICommand? GetCommand(string rawInput, out int threshold)
    {
        var segments = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        CommandGroupAttribute? parent = null;
        threshold = int.MaxValue;
        for (int i = 0; i < segments.Length; i++)
        {
            ICommand? result = MatchCommand(parent, segments[i], out threshold);

            if (result == null || threshold > ExactMatchThreshold)
            {
                StringBuilder compositeSegment = new(segments[i]);
                for (int j = i + 1; j < segments.Length; j++)
                {
                    _ = compositeSegment.Append(' ').Append(segments[j]);

                    ICommand? newResult = MatchCommand(parent, compositeSegment.ToString(), out int newThreshold);

                    if (newThreshold < threshold)
                    {
                        threshold = newThreshold;
                        result = newResult;
                    }
                }

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
        var segments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        StringBuilder normalizer = new();
        foreach (var segment in segments)
        {
            if (normalizer.Length == 0)
            {
                _ = normalizer.Append(segment);
            }
            else
            {
                _ = normalizer.Append(' ').Append(segment);
            }
        }

        var normalizedInput = normalizer.ToString();

        return normalizedInput[command.FullName.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private static object ConvertArgument(string argument, Type targetType)
    {
        return targetType.IsEnum
        ? Enum.Parse(targetType, argument, ignoreCase: true)
        : Convert.ChangeType(argument, targetType);
    }
}
