using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameServer;

#pragma warning disable SYSLIB1054

/// <summary>
/// Represents a precise timer.
/// </summary>
internal class PreciseTimer
{
    /// <summary>
    /// Delays the current thread for the specified number of milliseconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to delay the current thread.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task PreciseDelay(int milliseconds)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _ = TimeBeginPeriod(1);
        }

        var sw = Stopwatch.StartNew();
        var sleepMs = milliseconds - 1;

        if (sleepMs > 0)
        {
            await Task.Delay(sleepMs);
        }

        while (sw.ElapsedMilliseconds < milliseconds)
        {
            Thread.SpinWait(1);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _ = TimeEndPeriod(1);
        }
    }

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
    private static extern uint TimeBeginPeriod(uint uMilliseconds);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
    private static extern uint TimeEndPeriod(uint uMilliseconds);
}
