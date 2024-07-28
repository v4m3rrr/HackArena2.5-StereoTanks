using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;

namespace GameServer;

/// <summary>
/// Represents the command line parser.
/// </summary>
internal static partial class CommandLineParser
{
    /// <summary>
    /// Parses the command line arguments.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>
    /// The parsed options or <see langword="null"/> if the options are invalid.
    /// </returns>
    public static CommandLineOptions? Parse(string[] args)
    {
        var parser = new Parser(with =>
        {
            with.HelpWriter = null;
        });

        var parserResult = parser.ParseArguments<CommandLineOptions>(args);

        bool areOptionsValid = true;
        _ = parserResult.WithParsed((opts) =>
        {
            if (!RunOptions(opts))
            {
                areOptionsValid = false;
                var helpText = GenerateHelpText(parserResult);
                Console.WriteLine(helpText);
            }
        });

        if (!areOptionsValid)
        {
            return null;
        }

        _ = parserResult.WithNotParsed(
            errs =>
            {
                var helpText = GenerateHelpText(parserResult);
                Console.WriteLine(helpText);
            });

        return parserResult.Value;
    }

    [GeneratedRegex(@"^\*$|^localhost$|^(\d{1,3}\.){3}\d{1,3}$")]
    private static partial Regex HostRegex();

    private static bool RunOptions(CommandLineOptions opts)
    {
        if (!HostRegex().IsMatch(opts.Host))
        {
            Console.WriteLine("Invalid host. Must be a valid IP address or 'localhost'.");
            return false;
        }

        if (opts.Port is < 1 or > 65535)
        {
            Console.WriteLine("Invalid port. Must be between 1 and 65535.");
            return false;
        }

        if (opts.NumberOfPlayers is < 2 or > 4)
        {
            Console.WriteLine("Invalid number of players. Must be between 2 and 4.");
            return false;
        }

        if (opts.BroadcastInterval <= 0)
        {
            Console.WriteLine("Invalid broadcast interval. Must be at least 1.");
            return false;
        }

        return true;
    }

    private static HelpText GenerateHelpText(ParserResult<CommandLineOptions> parserResult)
    {
        return HelpText.AutoBuild(
            parserResult,
            h =>
            {
                h.AutoVersion = false;

                h.Heading = Assembly
                    .GetEntryAssembly()!
                    .GetCustomAttribute<AssemblyTitleAttribute>()?
                    .Title;

                h.Copyright = "Copyright 2024" + Assembly
                    .GetEntryAssembly()!
                    .GetCustomAttribute<AssemblyCompanyAttribute>()?
                    .Company;

                var descrption = Assembly
                    .GetEntryAssembly()!
                    .GetCustomAttribute<AssemblyDescriptionAttribute>()?
                    .Description;
                _ = h.AddPreOptionsLine($"\n{descrption}");

                var assemblyName = Assembly
                    .GetEntryAssembly()!
                    .GetName()
                    .Name;
                _ = h.AddPostOptionsLine($"Usage: {assemblyName}.exe -- [options]");

                return h;
            },
            e => e);
    }
}
