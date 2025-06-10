using System.Diagnostics;

namespace GameLogic;

public class Bot
{
    private const string ExePath = "docker";

    private readonly TankType tankType;
    private readonly string teamName;
    private readonly Difficulty difficulty;

    private Process? process;

    public Bot(string teamName, TankType tankType, Difficulty difficulty)
    {
        this.teamName = teamName;
        this.tankType = tankType;
        this.difficulty = difficulty;
    }

    public void Start()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ExePath,
            Arguments = GetCommand(this.difficulty, this.teamName, this.tankType.ToString().ToLower()),
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

    private static string GetCommand(Difficulty difficulty, string teamName, string tankType)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return $"run --rm wrapper --host host.docker.internal --team-name {teamName} --tank-type {tankType}";
            case Difficulty.Medium:
                return $"run --rm wrapper --host host.docker.internal --team-name {teamName} --tank-type {tankType}";
            case Difficulty.Hard:
                return $"run --rm wrapper --host host.docker.internal --team-name {teamName} --tank-type {tankType}";
        }

        return $"run --rm wrapper --host host.docker.internal --team-name {teamName} --tank-type {tankType}";
    }
}
