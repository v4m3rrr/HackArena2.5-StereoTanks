using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Fastenshtein;
using GameClient.DebugConsoleItems;
using Microsoft.Xna.Framework;

namespace GameClient.Source;

/// <summary>
/// Provides parser.
/// </summary>
internal static class CommandParser
{
    private static readonly int CommandTreshold = 3;

    /// <summary>
    /// Parse method that recieves data from event.
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

        GetCommand(e.Value);
        return;
        /*
        ICommand? command = GetCommandFromInput(e.Value, out int threshold);

        if (command is null || threshold > CommandTreshold)
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
                    // Arguments are invalid so we return.
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

                c.Action.DynamicInvoke(invokeParameters.ToArray());
            }
            catch (Exception ex) when (
                ex is TargetParameterCountException
                || ex is System.IndexOutOfRangeException)
            {
                DebugConsole.SendMessage($"Invalid number of arguments. (expected: {parameters.Length}, got: {args.Length})", Color.IndianRed);
            }
            catch (System.IndexOutOfRangeException)
            {
                DebugConsole.SendMessage($"Invalid number of arguments. (expected: {parameters.Length}, got: {args.Length})", Color.IndianRed);
            }

            return;
        }

        DebugConsole.SendMessage($"Did you mean '{command.FullName}'?", Color.Orange);
        */
    }

    private static ICommand? MatchCommand(string input, out int threshold)
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
    //private static ICommand? GetCommand(string rawInput, out int newthreshold)

    private static void GetCommand(string rawInput)
    {
        var segments = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder prefix = new();
        for (int i = 0; i < segments.Length; i++)
        {
            _ = prefix.Append(segments[i]);

            ICommand? bestMatch = MatchCommand(prefix.ToString(), out int threshold);

            DebugConsole.SendMessage($"for prefix: {prefix.ToString()} best mach is: {bestMatch.DisplayName} with dist: {threshold}", Color.White);
        }
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
}
