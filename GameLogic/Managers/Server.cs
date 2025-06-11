using System.Diagnostics;

namespace GameLogic;

public class Server
{
#if DEBUG
    private const string ExePath = @"..\..\..\..\..\..\GameServer\bin\StereoDebug\Windows\x64\net8.0\GameServer.exe";
#else
    private const string ExePath = @"..\..\..\..\..\..\GameServer\bin\StereoRelease\Windows\x64\net8.0\GameServer.exe";
#endif
    //private const string Arguments = "--host *";
    private const string Arguments = "--host ";

    private Process? process;

    public void Start(string host, string port)
    {

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ExePath,
            Arguments = Arguments + host + " --port " + port,
            UseShellExecute = false,
#if DEBUG
            CreateNoWindow = false,
#else
            CreateNoWindow = true,
#endif
            //Verb = "runas",
        };
        this.process = new Process
        {
            StartInfo = startInfo,
        };
        _ = this.process.Start();
    }

    public void Stop()
    {
        if (this.process != null && !this.process.HasExited)
        {
            this.process.Kill();
            this.process.Dispose();
            this.process = null;
        }
    }
}
