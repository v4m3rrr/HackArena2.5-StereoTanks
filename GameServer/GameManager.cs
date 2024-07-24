using System.Collections.Concurrent;

namespace GameServer;

internal class GameManager
{
    private readonly ConcurrentDictionary<string, GameInstance> games = [];

    public GameInstance CreateGame()
    {
        string gameId;
        do
        {
            gameId = Guid.NewGuid().ToString();
        } while (this.games.ContainsKey(gameId));

        string joinCode = this.GenerateJoinCode(gameId);
        var gameInstance = new GameInstance(gameId) { JoinCode = joinCode };
        gameInstance.Grid.GenerateWalls();
        this.games[gameId] = gameInstance;
        return gameInstance;
    }

    public GameInstance? GetGameById(string gameId)
    {
        return this.games.TryGetValue(gameId, out var gameInstance) ? gameInstance : null;
    }

    public GameInstance? GetGameByJoinCode(string joinCode)
    {
        return this.games.Values.FirstOrDefault(g => g.JoinCode == joinCode);
    }

    public void RemoveGame(string gameId)
    {
        _ = this.games.TryRemove(gameId, out _);
    }

    public ICollection<GameInstance> GetAllGames()
    {
        return this.games.Values;
    }

    private string GenerateJoinCode(string gameId)
    {
        const int LENGTH = 4;
        string joinCode = gameId[^LENGTH..];

        while (this.games.Values.Any(g => g.JoinCode == joinCode))
        {
            joinCode = Guid.NewGuid().ToString()[^LENGTH..];
        }

        return joinCode;
    }
}
