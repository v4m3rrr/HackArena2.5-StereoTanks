using System.Diagnostics;

#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace GameServer;

#pragma warning disable IDE0079
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
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PreciseDelay(int milliseconds)
    {
#if WINDOWS
        _ = TimeBeginPeriod(1);
#endif

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

#if WINDOWS
        _ = TimeEndPeriod(1);
#endif
    }

#if WINDOWS

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
    private static extern uint TimeBeginPeriod(uint uMilliseconds);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
    private static extern uint TimeEndPeriod(uint uMilliseconds);

#endif
}
