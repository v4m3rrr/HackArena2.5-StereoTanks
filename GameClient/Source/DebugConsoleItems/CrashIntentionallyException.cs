using System;

namespace GameClient.DebugConsoleItems;

/// <summary>
/// Represents an exception that is thrown when the game is crashed intentionally.
/// </summary>
internal class CrashIntentionallyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CrashIntentionallyException"/> class.
    /// </summary>
    public CrashIntentionallyException()
        : base("The game has been crashed intentionally")
    {
    }
}
