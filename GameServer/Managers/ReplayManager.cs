using System.IO.Compression;
using GameLogic.Networking;
using Newtonsoft.Json.Linq;
using Serilog.Core;

namespace GameServer;

/// <summary>
/// Represents the replay manager.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="replayPath">The path to save the replay.</param>
/// <param name="log">The logger.</param>
internal class ReplayManager(GameInstance game, string replayPath, Logger log)
{
    private readonly List<JObject> gameStates = [];
    private JObject? lobbyData;
    private JObject? gameEnd;

    /// <summary>
    /// Saves the lobby data.
    /// </summary>
    public void SaveLobbyData()
    {
        log.Verbose("Replay - saving lobby data...");

        var payload = game.PayloadHelper.GetLobbyDataPayload(null, out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.lobbyData = JObject.FromObject(payload, serializer);

        log.Verbose("Replay - lobby data saved.");
    }

    /// <summary>
    /// Adds a game state.
    /// </summary>
    /// <param name="tick">The tick of the game state.</param>
    /// <param name="gameStateId">The game state id.</param>
    public void AddGameState(int tick, string gameStateId)
    {
        log.Verbose("Replay - adding game state ({tick})...", tick);

        var payload = game.PayloadHelper.GetGameStatePayload(null, tick, gameStateId, out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.gameStates.Add(JObject.FromObject(payload, serializer));

        log.Verbose("Replay - game state added ({tick}).", tick);
    }

    /// <summary>
    /// Saves the game end.
    /// </summary>
    public void SaveGameEnd()
    {
        log.Verbose("Replay - saving game end...");

        var payload = game.PayloadHelper.GetGameEndPayload(out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.gameEnd = JObject.FromObject(payload, serializer);

        log.Verbose("Replay - game end saved.");
    }

    /// <summary>
    /// Saves the replay.
    /// </summary>
    public void SaveReplay()
    {
        var options = new SerializationOptions() { Formatting = Newtonsoft.Json.Formatting.None };

        if (this.lobbyData is null)
        {
            log.Warning("Saving replay with missing lobby data.");
        }

        if (this.gameStates.Count < game.Settings.Ticks)
        {
            log.Warning(
                "Saving replay with missing game states. Expected: {expected}; Actual: {actual}",
                game.Settings.Ticks,
                this.gameStates.Count);
        }

        if (this.gameEnd is null)
        {
            log.Warning("Saving replay with missing game end.");
        }

        var jObject = new JObject()
        {
            ["lobbyData"] = this.lobbyData ?? [],
            ["gameStates"] = JArray.FromObject(this.gameStates),
            ["gameEnd"] = this.gameEnd ?? [],
        };

        var replayFileNameWithoutExtension = Path.GetFileNameWithoutExtension(replayPath);
        var replayFileExtension = replayPath.EndsWith(".tar.gz") ? ".tar.gz" : Path.GetExtension(replayPath);

        try
        {
            log.Debug("Saving replay to: {replayPath}", replayPath);

            if (replayFileExtension == ".zip")
            {
                log.Debug("Zipping replay...");
                using var zip = ZipFile.Open(replayPath, ZipArchiveMode.Create);
                var entryFileName = $"{replayFileNameWithoutExtension}.json";
                var replayEntry = zip.CreateEntry(entryFileName);
                using var entryStream = replayEntry.Open();
                using var writer = new StreamWriter(entryStream);
                writer.Write(jObject.ToString(options.Formatting));
            }
            else if (replayFileExtension == ".tar.gz")
            {
                log.Debug("Creating tar.gz archive...");
                using var replayFileStream = File.Create(replayPath);
                using var compressionStream = new GZipStream(replayFileStream, CompressionMode.Compress);
                using var writer = new StreamWriter(compressionStream);
                writer.Write(jObject.ToString(options.Formatting));
            }
            else
            {
                File.WriteAllText(replayPath, jObject.ToString(options.Formatting));
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "Saving replay failed.");
            return;
        }

        log.Information("Replay saved to: {replayPath}", replayPath);
    }

#if HACKATHON

    /// <summary>
    /// Saves the game results.
    /// </summary>
    /// <param name="results">The game results.</param>
    /// <remarks>
    /// This method is only available in the hackathon mode.
    /// </remarks>
    public void SaveResults()
    {
        log.Debug("Saving results...");

        // Temporary solution for getting the results.
        var gameEndPayload = game.PayloadHelper.GetGameEndResultsPayload(out var converters);
        var options = new SerializationOptions() { Formatting = Newtonsoft.Json.Formatting.None };
        _ = PacketSerializer.Serialize(gameEndPayload, out var results, converters, options);

        string? savePath;
        try
        {
            var path = Path.GetDirectoryName(replayPath)!;
            var fileName = Path.GetFileNameWithoutExtension(replayPath);
            savePath = Path.Combine(path, $"{fileName}_results.json");

            log.Debug("Saving results to: {savePath}", savePath);
            File.WriteAllText(savePath, results.ToString());
        }
        catch (Exception ex)
        {
            log.Error(ex, "Saving results failed.");
            return;
        }

        log.Information("Results saved to: {savePath}", savePath);
    }

#endif
}
