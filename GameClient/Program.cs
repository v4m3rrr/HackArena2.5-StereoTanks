#if !DEBUG
var currentDomain = System.AppDomain.CurrentDomain;
currentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(GameClient.CrashService.HandleCrash);
#endif

using var game = new GameClient.MonoTanks();
game.Run();
