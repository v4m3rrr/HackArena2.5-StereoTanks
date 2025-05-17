using System.Collections.Generic;
using System.IO.Compression;
using GameLogic.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace GameServer;

/// <summary>
/// Represents the replay manager.
/// </summary>
internal class ReplayManager
{
    private const int FlushIntervalTicks = 10;
    private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "StereoTanks_Replays");

    private readonly GameInstance game;
    private readonly string replayPath;
    private readonly ILogger logger;

    private JObject? lobbyData;
    private JObject? gameEnd;
    private StreamWriter? replayWriter;
    private string? tempJsonPath;
    private bool isFirstState = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplayManager"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    /// <param name="replayPath">The path to save the replay.</param>
    /// <param name="logger">The logger.</param>
    public ReplayManager(GameInstance game, string replayPath, ILogger logger)
    {
        this.game = game;
        this.replayPath = replayPath;
        this.logger = logger;

        if (string.IsNullOrEmpty(replayPath))
        {
            throw new ArgumentException("Replay path cannot be null or empty.", nameof(replayPath));
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) => this.TryDeleteTemp();
    }

    /// <summary>
    /// Starts the replay stream by opening a JSON file and writing the lobby data.
    /// </summary>
    public void SaveLobbyData()
    {
        this.logger.Verbose("Replay - saving lobby data...");

        var payload = this.game.PayloadHelper.GetLobbyDataPayload(null, out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.lobbyData = JObject.FromObject(payload, serializer);

        _ = Directory.CreateDirectory(TempDir);
        this.tempJsonPath = Path.Combine(TempDir, $"replay_{Guid.NewGuid():N}.json");

        this.replayWriter = new StreamWriter(File.Open(this.tempJsonPath, FileMode.Create, FileAccess.Write, FileShare.None));

        this.replayWriter.Write("{\"lobbyData\":");
        this.replayWriter.Write(this.lobbyData.ToString(Newtonsoft.Json.Formatting.None));
        this.replayWriter.Write(",\"gameStates\":[");

        this.logger.Verbose("Replay - lobby data saved.");

        this.replayWriter?.Flush();
        this.logger.Verbose("Replay - lobby data flushed.");
    }

    /// <summary>
    /// Adds a game state to the stream.
    /// </summary>
    /// <param name="tick">The tick of the game state.</param>
    /// <param name="gameStateId">The game state id.</param>
    public void AddGameState(int tick, string gameStateId)
    {
        if (this.replayWriter is null)
        {
            throw new InvalidOperationException("Replay stream is not initialized. Call SaveLobbyData first.");
        }

        this.logger.Verbose("Replay - adding game state ({tick})...", tick);

        var payload = this.game.PayloadHelper.GetGameStatePayload(null, tick, gameStateId, out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        var gameState = JObject.FromObject(payload, serializer).ToString(Formatting.None);

        if (!this.isFirstState)
        {
            this.replayWriter.Write(',');
        }

        this.replayWriter.Write(gameState);
        this.isFirstState = false;

        this.logger.Verbose("Replay - game state added ({tick}).", tick);

        if (tick % FlushIntervalTicks == 0)
        {
            this.replayWriter.Flush();
            this.logger.Verbose("Replay - game state flushed ({tick}).", tick);
        }
    }

    /// <summary>
    /// Saves the game end payload.
    /// </summary>
    public void SaveGameEnd()
    {
        this.logger.Verbose("Replay - saving game end...");

        var payload = this.game.PayloadHelper.GetGameEndPayload(null, out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.gameEnd = JObject.FromObject(payload, serializer);

        this.logger.Verbose("Replay - game end saved.");

        this.replayWriter?.Flush();
        this.logger.Verbose("Replay - game end flushed.");
    }

    /// <summary>
    /// Finalizes and saves the replay (optionally compressed).
    /// </summary>
    public void SaveReplay()
    {
        if (this.replayWriter is null || this.tempJsonPath is null)
        {
            this.logger.Warning("Replay stream not initialized. Replay will not be saved.");
            return;
        }

        if (this.gameEnd is null)
        {
            this.logger.Warning("Saving replay with missing game end.");
        }

        try
        {
            this.replayWriter.Write("],\"gameEnd\":");
            this.replayWriter.Write(this.gameEnd?.ToString(Newtonsoft.Json.Formatting.None) ?? "{}");
            this.replayWriter.Write('}');
            this.replayWriter.Dispose();
            this.replayWriter = null;

            this.logger.Debug("Replay written to temp file: {path}", this.tempJsonPath);

            if (this.replayPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                this.logger.Debug("Creating zip archive...");
                using var zip = ZipFile.Open(this.replayPath, ZipArchiveMode.Create);
                var entryName = Path.GetFileName(this.tempJsonPath);
                _ = zip.CreateEntryFromFile(this.tempJsonPath, entryName);
                File.Delete(this.tempJsonPath);
            }
            else if (this.replayPath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            {
                this.logger.Debug("Creating tar.gz archive...");
                using var fileStream = File.Create(this.replayPath);
                using var gzip = new GZipStream(fileStream, CompressionMode.Compress);
                using var input = File.OpenRead(this.tempJsonPath);
                input.CopyTo(gzip);
                File.Delete(this.tempJsonPath);
            }
            else
            {
                File.Move(this.tempJsonPath, this.replayPath, overwrite: true);
            }
        }
        catch (Exception ex)
        {
            this.logger.Error(ex, "Saving replay failed.");
            return;
        }

        this.logger.Information("Replay saved to: {replayPath}", this.replayPath);
    }

#if HACKATHON

    /// <summary>
    /// Saves the game results.
    /// </summary>
    /// <remarks>
    /// This method is only available in the hackathon mode.
    /// </remarks>
    public void SaveResults()
    {
        this.logger.Debug("Saving results...");

        // Temporary solution for getting the results.
        var gameEndPayload = this.game.PayloadHelper.GetGameEndResultsPayload(out var converters);
        var options = new SerializationOptions() { Formatting = Newtonsoft.Json.Formatting.None };
        _ = PacketSerializer.Serialize(gameEndPayload, out var results, converters, options);

        string? savePath;
        try
        {
            var path = Path.GetDirectoryName(this.replayPath)!;
            var fileName = Path.GetFileNameWithoutExtension(this.replayPath);
            savePath = Path.Combine(path, $"{fileName}_results.json");

            this.logger.Debug("Saving results to: {savePath}", savePath);
            File.WriteAllText(savePath, results.ToString());
        }
        catch (Exception ex)
        {
            this.logger.Error(ex, "Saving results failed.");
            return;
        }

        this.logger.Information("Results saved to: {savePath}", savePath);
    }

#endif

    private void TryDeleteTemp()
    {
        this.replayWriter?.Dispose();
        this.replayWriter = null;

        try
        {
            if (!string.IsNullOrEmpty(this.tempJsonPath) && File.Exists(this.tempJsonPath))
            {
#if DEBUG
                var invalidPath = Path.ChangeExtension(this.tempJsonPath, ".INVALID.json");
                File.Move(this.tempJsonPath, invalidPath, overwrite: true);
#else
                File.Delete(this.tempJsonPath);
#endif
            }
        }
        catch
        {
            /* ignore */
        }
    }
}
