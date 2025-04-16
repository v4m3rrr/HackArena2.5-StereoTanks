global using System.Diagnostics;
using GameClient.Networking;

namespace GameClient;

/// <summary>
/// Represents the entry point for the game client application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        var currentDomain = AppDomain.CurrentDomain;

#if !DEBUG
        currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashService.HandleCrash);
#endif

        currentDomain.ProcessExit += async (s, e) =>
        {
            if (ServerConnection.IsConnected)
            {
                await ServerConnection.CloseAsync("Client exited");
            }
        };

        using var game = new GameClientCore();
        game.Run();
    }
}
