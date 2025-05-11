using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GameClient.Networking;

#if WINDOWS
using System.Management;
#endif

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

        var assembly = typeof(GameClientCore).Assembly;
        var version = assembly.GetName().Version!;
        var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration;
        var exceptionType = (e.ExceptionObject as Exception)?.GetType().Name ?? "Unknown";
        var exceptionMessage = (e.ExceptionObject as Exception)?.Message ?? "Unknown";

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
            .Append(GameClientCore.Platform)
            .Append(") [")
            .Append(configuration)
            .Append(']')
            .ToString();

        var uptime = DateTime.Now - GameClientCore.AppStartTime;
        var memoryUsage = Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024; // MB
        var cpuCores = Environment.ProcessorCount;
        var osDescription = RuntimeInformation.OSDescription;
        var osArchitecture = RuntimeInformation.OSArchitecture.ToString();

        var sb = new StringBuilder()
            .AppendLine("Please report this issue to the developers.")
            .AppendLine("Include the following information in your report.")
            .AppendLine("-------------------------------------------------")
            .AppendLine($"Version: {v}")
            .AppendLine($"Operating System: {osDescription} ({osArchitecture})")
            .AppendLine($"Processor Cores: {cpuCores}")
            .AppendLine($"RAM: {GetRamUsage()}")
            .AppendLine($"Memory Usage: {memoryUsage} MB")
            .AppendLine($"Uptime: {uptime}");

        string gpuInfo = "Unknown";

#if WINDOWS
        try
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_VideoController");
            foreach (var mo in mos.Get().Cast<ManagementObject>())
            {
                gpuInfo = mo["Name"]?.ToString() ?? "Unknown";
                break;
            }
        }
        catch (Exception gpuException)
        {
            gpuInfo = $"Failed to get GPU info: {gpuException}";
        }
#endif

        sb.AppendLine($"GPU: {gpuInfo}")
            .AppendLine($"IsTerminating: {e.IsTerminating}")
            .AppendLine($"Exception: {exceptionType}");

        if (DebugConsole.Instance is { } dc && dc.IsContentLoaded)
        {
            sb.AppendLine("-------------------------------------------------")
                .AppendLine("Recent 200 Debug Messages:");

            var debugMessages = DebugConsole.Instance.GetRecentMessages(200);
            debugMessages.Reverse();
            foreach (var message in debugMessages)
            {
                sb.AppendLine(message);
            }
        }
        else
        {
            sb.AppendLine("Debug Console is not available.");
        }

        sb.AppendLine("-------------------------------------------------")
            .AppendLine("Stack Trace:")
            .AppendLine(GetFullExceptionDetails(e.ExceptionObject));

        sb.AppendLine("Apologies for the inconvenience and thank you for your cooperation!");

        var sb2 = new StringBuilder()
            .AppendLine("Foock...")
            .AppendLine("Probably a crash.")
            .AppendLine("Don't worry, we'll get through this together.\n")
            .AppendLine($"Exception: {exceptionType}\n")
            .AppendLine("Please report this issue to the developers.")
            .AppendLine("-----------------------------------------------------");

        try
        {
            File.WriteAllText(filepath, sb.ToString());
            sb2.AppendLine($"The crash log has been saved to the \"{Path.GetFullPath(filepath)}\" file.");
        }
        catch (Exception fileException)
        {
            sb2.AppendLine("Failed to write to log file!")
                .AppendLine($"Log File Exception: {fileException}")
                .AppendLine("-----------------------------------------------------")
                .AppendLine("Apologies for the inconvenience!");
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

    private static string GetFullExceptionDetails(object exceptionObject)
    {
        if (exceptionObject is not Exception ex)
        {
            return "No stack trace available.";
        }

        var sb = new StringBuilder();
        var currentException = ex;
        var exceptionNumber = 1;

        while (currentException != null)
        {
            _ = sb.AppendLine("-------------------------------------------------")
                .AppendLine($"Exception {exceptionNumber}: {currentException.GetType().Name}")
                .AppendLine($"Message: {currentException.Message}")
                .AppendLine("Stack Trace:")
                .AppendLine(currentException.StackTrace);

            currentException = currentException.InnerException;
            exceptionNumber++;
        }

        return sb.ToString();
    }

    private static string GetRamUsage()
    {
        try
        {
#if WINDOWS

            var availableBytesCounter = new PerformanceCounter("Memory", "Available Bytes");
            long? totalMemory = null;

            var query = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            using var searcher = new ManagementObjectSearcher(query);
            foreach (var result in searcher.Get())
            {
                totalMemory = Convert.ToInt64(result["TotalPhysicalMemory"]);
                break;
            }

            var availableMemory = availableBytesCounter.RawValue;
            var usedMemory = totalMemory!.Value - availableMemory;

            return $"{usedMemory / (1024 * 1024)} / {totalMemory / (1024 * 1024)} MB";

#elif LINUX

            var memInfo = ExecuteBashCommand("grep MemTotal /proc/meminfo && grep MemAvailable /proc/meminfo");
            var totalMemory = ParseMemory(memInfo.Split('\n')[0]);
            var availableMemory = ParseMemory(memInfo.Split('\n')[1]);
            var usedMemory = totalMemory - availableMemory;
            return $"{usedMemory} / {totalMemory} MB";

#elif OSX

            var totalMemory = ExecuteBashCommand("sysctl hw.memsize");
            var usedMemory = ExecuteBashCommand("vm_stat | grep 'Pages active' | awk '{print $3}' | sed 's/\\.$//'");
            var totalMemoryMB = ParseMemory(totalMemory);
            var usedMemoryMB = ulong.Parse(usedMemory) * 4096 / (1024 * 1024); // 1 page = 4 KB
            return $"Used: {usedMemoryMB} / {totalMemoryMB} MB";
#endif

        }
        catch (Exception ex)
        {
            return $"Error retrieving RAM info: {ex.Message}";
        }
    }

#if LINUX || OSX

    private static string ExecuteBashCommand(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        _ = process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }

#endif

#if LINUX || OSX

    private static ulong ParseMemory(string output)
    {
        char[] separators = [' ', '\t'];
        var parts = output.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && ulong.TryParse(parts[1], out var mem))
        {
#if LINUX
            return mem / 1024;
#elif OSX
            return mem / (1024 * 1024);
#endif
        }

        return 0;
    }

#endif
}
