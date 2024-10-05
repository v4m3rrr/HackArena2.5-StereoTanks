global using System.Diagnostics;
using GameClient.Networking;

#if !DEBUG
    var currentDomain = System.AppDomain.CurrentDomain;
    currentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(GameClient.CrashService.HandleCrash);
#endif

System.AppDomain.CurrentDomain.ProcessExit += async(s, e) =>
{
    if (ServerConnection.IsConnected)
    {
        await ServerConnection.CloseAsync("Client exited");
    }
};

using var game = new GameClient.MonoTanks();
game.Run();
