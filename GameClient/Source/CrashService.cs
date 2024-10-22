using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameClient.Networking;

namespace GameClient;

/// <summary>
/// Represents a crash service.
/// </summary>
internal static class CrashService
{
    private static readonly string Directory = "crash_logs";

    /// <summary>
    /// Handles the crash.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    public static async void HandleCrash(object sender, UnhandledExceptionEventArgs e)
    {
        Task? closeTask = null;
        if (ServerConnection.IsEstablished)
        {
            closeTask = ServerConnection.CloseAsync("Client crashed.");
        }

        if (!System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.CreateDirectory(Directory);
        }

        var filepath = PathUtils.GetAbsolutePath(
            $"{Directory}/crash-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");

        var assembly = typeof(MonoTanks).Assembly;
        var version = assembly.GetName().Version!;
        var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;

        var v = new StringBuilder()
            .Append('v')
            .Append(version.Major)
            .Append('.')
            .Append(version.Minor)
            .Append('.')
            .Append(version.Build)
            .Append('.')
            .Append(version.Revision)
            .Append(" (")
            .Append(configuration)
            .Append(')')
            .ToString();

        var sb = new StringBuilder()
            .AppendLine("Foock...")
            .AppendLine("Probably a crash. Don't worry, we'll get through this together.\n")
            .AppendLine("Please report this issue to the developers.")
            .AppendLine("Include the following information in your report:")
            .AppendLine("--------------------------------------------------")
            .AppendLine($"Version: {v}")
            .AppendLine($"Exception: {e.ExceptionObject}")
            .AppendLine($"IsTerminating: {e.IsTerminating}")
            .AppendLine("--------------------------------------------------")
            .AppendLine("Thank you for your cooperation.");

        var sb2 = new StringBuilder()
            .AppendLine("Foock...")
            .AppendLine("Probably a crash. Don't worry, we'll get through this together.\n")
            .AppendLine("Please report this issue to the developers.")
            .AppendLine("--------------------------------------------------")
            .AppendLine($"The crash log has been saved to the \"{Path.GetFullPath(filepath)}\" file.")
            .AppendLine("--------------------------------------------------")
            .AppendLine("Apologies for the inconvenience!");

        try
        {
            File.WriteAllText(filepath, sb.ToString());
        }
        catch (Exception fileException)
        {
            _ = sb2.AppendLine("--------------------------------------------------")
              .AppendLine("Failed to write to log file.")
              .AppendLine($"Log File Exception: {fileException}");
        }

#if WINDOWS
        System.Windows.Forms.MessageBox.Show(
            sb2.ToString(),
            "Crash Report",
            System.Windows.Forms.MessageBoxButtons.OK,
            System.Windows.Forms.MessageBoxIcon.Error);
#endif
        if (closeTask != null)
        {
            await closeTask;
        }
    }
}
