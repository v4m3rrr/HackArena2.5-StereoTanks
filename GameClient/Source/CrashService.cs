using System;
using System.IO;
using System.Text;

namespace GameClient;

/// <summary>
/// Represents a crash service.
/// </summary>
internal static class CrashService
{
    private const string LogFilePath = "crash_report.log";

    /// <summary>
    /// Handles the crash.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    public static void HandleCrash(object sender, UnhandledExceptionEventArgs e)
    {
        var sb = new StringBuilder()
            .AppendLine("Foock...")
            .AppendLine("Probably a crash. Don't worry, we'll get through this together.\n")
            .AppendLine("Please report this issue to the developers.")
            .AppendLine("Include the following information in your report:")
            .AppendLine("--------------------------------------------------")
            .AppendLine($"Exception: {e.ExceptionObject}")
            .AppendLine($"IsTerminating: {e.IsTerminating}")
            .AppendLine("--------------------------------------------------")
            .AppendLine("Thank you for your cooperation.");

        var sb2 = new StringBuilder()
            .AppendLine("Foock...")
            .AppendLine("Probably a crash. Don't worry, we'll get through this together.\n")
            .AppendLine("Please report this issue to the developers.")
            .AppendLine("--------------------------------------------------")
            .AppendLine($"The crash log has been saved to the \"{Path.GetFullPath(LogFilePath)}\" file.")
            .AppendLine("--------------------------------------------------")
            .AppendLine("Apologies for the inconvenience!");

        try
        {
            File.WriteAllText(LogFilePath, sb.ToString());
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
    }
}
