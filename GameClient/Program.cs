global using System.Diagnostics;
using GameClient.Networking;

namespace MonoTanks;

/// <summary>
/// Represents the MonoTanks game client.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [System.STAThread]
    public static void Main()
    {
        var currentDomain = System.AppDomain.CurrentDomain;

#if !DEBUG
        currentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(GameClient.CrashService.HandleCrash);
#endif

        currentDomain.ProcessExit += async (s, e) =>
        {
            if (ServerConnection.IsConnected)
            {
                await ServerConnection.CloseAsync("Client exited");
            }
        };

        using var game = new GameClient.MonoTanks();
        game.Run();
    }
}
