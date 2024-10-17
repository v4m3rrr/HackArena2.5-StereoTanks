using GameLogic.Networking;
using Newtonsoft.Json.Linq;

namespace GameServer;

/// <summary>
/// Represents the replay manager.
/// </summary>
/// <param name="game">The game instance.</param>
/// <param name="replayPath">The path to save the replay.</param>
internal class ReplayManager(GameInstance game, string replayPath)
{
    private readonly List<JObject> gameStates = [];
    private JObject? lobbyData;
    private JObject? gameEnd;

    /// <summary>
    /// Saves the lobby data.
    /// </summary>
    public void SaveLobbyData()
    {
        var payload = game.PayloadHelper.GetLobbyDataPayload(null, out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.lobbyData = JObject.FromObject(payload, serializer);
    }

    /// <summary>
    /// Adds a game state.
    /// </summary>
    /// <param name="tick">The tick of the game state.</param>
    /// <param name="gameStateId">The game state id.</param>
    public void AddGameState(int tick, string gameStateId)
    {
        var payload = game.PayloadHelper.GetGameStatePayload(null, tick, gameStateId, out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.gameStates.Add(JObject.FromObject(payload, serializer));
    }

    /// <summary>
    /// Saves the game end.
    /// </summary>
    public void SaveGameEnd()
    {
        var payload = game.PayloadHelper.GetGameEndPayload(out var converters);
        var serializer = PacketSerializer.GetSerializer(converters);
        this.gameEnd = JObject.FromObject(payload, serializer);
    }

    /// <summary>
    /// Saves the replay.
    /// </summary>
    public void SaveReplay()
    {
        var options = new SerializationOptions() { Formatting = Newtonsoft.Json.Formatting.None };

        if (this.lobbyData is null)
        {
            Console.WriteLine("[ERROR] Saving replay failed!");
            Console.WriteLine("[^^^^^] Lobby data is missing.");
            return;
        }

        if (this.gameStates.Count < game.Settings.Ticks)
        {
            Console.WriteLine("[WARN] Saving replay with missing game states.");
            Console.WriteLine(
                "[^^^^] Expected: {0}; Actual: {1}",
                game.Settings.Ticks,
                this.gameStates.Count);
        }

        if (this.gameEnd is null)
        {
            Console.WriteLine("[ERROR] Saving replay failed!");
            Console.WriteLine("[^^^^^] Game end is missing.");
            return;
        }

        var jObject = new JObject()
        {
            ["lobbyData"] = this.lobbyData,
            ["gameStates"] = JArray.FromObject(this.gameStates),
            ["gameEnd"] = this.gameEnd,
        };

        try
        {
            File.WriteAllText(replayPath, jObject.ToString(options.Formatting));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Saving replay failed!");
            Console.WriteLine("[^^^^^] {0}", ex.Message);
            return;
        }

        Console.WriteLine("[INFO] Replay saved to:");
        Console.WriteLine("[^^^^] {0}", replayPath);
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
        // Temporary solution for getting the results.
        var gameEndPayload = game.PayloadHelper.GetGameEndResultsPayload(out var converters);
        var options = new SerializationOptions() { Formatting = Newtonsoft.Json.Formatting.None };
        _ = PacketSerializer.Serialize(gameEndPayload, out var results, converters, options);

        string? savePath = null;
        try
        {
            var path = Path.GetDirectoryName(replayPath)!;
            var fileName = Path.GetFileNameWithoutExtension(replayPath);
            var extension = Path.GetExtension(replayPath);
            savePath = Path.Combine(path, $"{fileName}_results{extension}");
            File.WriteAllText(savePath, results.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Saving results failed!");
            Console.WriteLine("[^^^^^] {0}", ex.Message);
        }

        Console.WriteLine("[INFO] Results saved to:");
        Console.WriteLine("[^^^^] {0}", savePath);
    }

#endif
}
