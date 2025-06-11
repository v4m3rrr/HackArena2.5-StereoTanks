using System.Diagnostics;
using System.Threading.Tasks;

namespace GameLogic;

public class Bot
{
    // For now only hard mode

    private readonly TankType tankType;
    private readonly string teamName;
    private readonly Difficulty difficulty;
    private readonly string host = "localhost";
    private readonly string port = "5000";

    private Process? process;

    public Bot(string teamName, TankType tankType, Difficulty difficulty, string host, string port)
    {
        this.teamName = teamName;
        this.tankType = tankType;
        this.difficulty = difficulty;
        this.host = host;
        this.port = port;
    }

    public async Task Start()
    {
        await Task.Delay(500); // Wait for the server to start who cares i dont
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = GetExePath(this.difficulty, this.teamName),
            Arguments = GetCommand(this.difficulty, this.teamName, this.tankType.ToString().ToLower(), this.host, this.port),
            UseShellExecute = false,
#if DEBUG
            CreateNoWindow = false,
#else
            CreateNoWindow = true,  
#endif
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

    private static string GetExePath(Difficulty difficulty, string teamName)
    {
        return $@"..\..\..\..\..\..\Bots\{difficulty.ToString()}\main.exe";
    }

    private static string GetCommand(Difficulty difficulty, string teamName, string tankType, string host, string port)
    {
        return $"--team-name {teamName} --tank-type {tankType} --host {host} --port {port}";
    }
}
