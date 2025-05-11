using GameLogic.Networking;
using Newtonsoft.Json;

namespace GameServer;

/// <summary>
/// A helper class for payloads.
/// </summary>
/// <param name="game">The game instance to get the payload for.</param>
internal class PayloadHelper(GameInstance game)
{
#if STEREO
    private List<GameLogic.Team> Teams => [..game.Teams];
#else
    private List<GameLogic.Player> Players => [..game.Players.Select(x => x.Instance)];
#endif

    /// <summary>
    /// Gets the lobby data payload.
    /// </summary>
    /// <param name="connection">The connection to get the lobby data for.</param>
    /// <returns>The lobby data payload.</returns>
    /// <remarks>
    /// If the connection is null, the payload will be
    /// for internal use (e.g. replay manager).
    /// </remarks>
    public LobbyDataPayload GetLobbyDataPayload(Connection? connection)
    {
        string? playerId = (connection as PlayerConnection)?.Instance.Id;
#if STEREO
        string? teamName = (connection as PlayerConnection)?.Instance.Team.Name;
        return new LobbyDataPayload(playerId, teamName, this.Teams, game.Settings);
#else
        return new LobbyDataPayload(playerId, this.Players, game.Settings);
#endif
    }

    /// <summary>
    /// Gets the lobby data payload.
    /// </summary>
    /// <param name="connection">The connection to get the lobby data for.</param>
    /// <param name="converters">The list of converters to serialize with.</param>
    /// <returns>The lobby data payload.</returns>
    /// <remarks>
    /// If the connection is null, the payload will be
    /// for internal use (e.g. replay manager).
    /// </remarks>
    public LobbyDataPayload GetLobbyDataPayload(Connection? connection, out List<JsonConverter> converters)
    {
        var context = connection is null
            ? SerializationContext.Default
            : new SerializationContext(connection.Data.EnumSerialization);

        converters = LobbyDataPayload.GetConverters(context);
        return this.GetLobbyDataPayload(connection);
    }

    /// <summary>
    /// Gets the game status payload.
    /// </summary>
    /// <returns>The game status payload.</returns>
    /// <remarks>
    /// If the game status is invalid, an error payload will be returned.
    /// </remarks>
    public IPacketPayload GetGameStatusPayload()
    {
        return game.GameManager.Status switch
        {
            GameStatus.InLobby => new EmptyPayload() { Type = PacketType.GameNotStarted },
            GameStatus.Starting => new EmptyPayload() { Type = PacketType.GameStarting },
            GameStatus.Running => new EmptyPayload() { Type = PacketType.GameInProgress },
            GameStatus.Ended => new EmptyPayload() { Type = PacketType.GameEnded },
            _ => new ErrorPayload(
                PacketType.InvalidPacketUsageError | PacketType.HasPayload,
                "Invalid game status."),
        };
    }

    /// <summary>
    /// Gets the game state payload.
    /// </summary>
    /// <param name="connection">The connection to get the game state for.</param>
    /// <param name="tick">The current tick of the game.</param>
    /// <param name="gameStateId">The game state id to get the payload for.</param>
    /// <returns>The game state payload.</returns>
    /// <remarks>
    /// If the connection is null, the payload will be
    /// for internal use (e.g. replay manager).
    /// </remarks>
    public GameStatePayload GetGameStatePayload(Connection? connection, int tick, string gameStateId)
    {
        return connection is PlayerConnection player
#if STEREO
            ? new GameStatePayload.ForPlayer(gameStateId, tick, player.Instance, this.Teams, game.Grid)
            : new GameStatePayload(tick, this.Teams, game.Grid);
#else
            ? new GameStatePayload.ForPlayer(gameStateId, tick, player.Instance, this.Players, game.Grid)
            : new GameStatePayload(tick, this.Players, game.Grid);
#endif
    }

    /// <summary>
    /// Gets the game state payload.
    /// </summary>
    /// <param name="connection">The connection to get the game state for.</param>
    /// <param name="tick">The current tick of the game.</param>
    /// <param name="gameStateId">The game state id to get the payload for.</param>
    /// <param name="converters">The list of converters to serialize with.</param>
    /// <returns>The game state payload.</returns>
    /// <remarks>
    /// If the connection is null, the payload will be
    /// for internal use (e.g. replay manager).
    /// </remarks>
    public GameStatePayload GetGameStatePayload(Connection? connection, int tick, string gameStateId, out List<JsonConverter> converters)
    {
        GameSerializationContext context = connection is PlayerConnection playerConnection
            ? new GameSerializationContext.Player(playerConnection.Instance, connection.Data.EnumSerialization)
            : new GameSerializationContext.Spectator();

        converters = GameStatePayload.GetConverters(context);
        return this.GetGameStatePayload(connection, tick, gameStateId);
    }

    /// <summary>
    /// Gets the game end payload.
    /// </summary>
    /// <returns>The game end payload.</returns>
    public GameEndPayload GetGameEndPayload()
    {
#if STEREO
        var teams = this.Teams;
        teams.Sort((x, y) => y.Score.CompareTo(x.Score));
        return new GameEndPayload(teams);
#else
        var players = this.Players;
        players.Sort((x, y) => y.Score.CompareTo(x.Score));
        return new GameEndPayload(players);
#endif
    }

    /// <summary>
    /// Gets the game end payload.
    /// </summary>
    /// <param name="connection">The connection to get the game end for.</param>
    /// <param name="converters">The list of converters to serialize with.</param>
    /// <returns>The game end payload.</returns>
    /// <remarks>
    /// If the connection is null, the payload will be
    /// for internal use (e.g. replay manager).
    /// </remarks>
    public GameEndPayload GetGameEndPayload(Connection? connection, out List<JsonConverter> converters)
    {
        var context = connection is null
            ? SerializationContext.Default
            : new SerializationContext(connection.Data.EnumSerialization);

        converters = GameEndPayload.GetConverters(context);
        return this.GetGameEndPayload();
    }

#if HACKATHON

    /// <summary>
    /// Gets the game end results payload.
    /// </summary>
    /// <param name="converters">The list of converters to serialize with.</param>
    /// <returns>The game end results payload.</returns>
    internal GameEndPayload GetGameEndResultsPayload(out List<JsonConverter> converters)
    {
        converters = GameEndPayload.GetConverters();

        bool isValid = !game.DisconnectedInGamePlayers.Any();
#if STEREO
        var teams = this.Teams;
        teams.Sort((x, y) => y.Score.CompareTo(x.Score));
        return new GameEndResultsPayload(teams, isValid);
#else
        var players = this.Players;
        players.AddRange(game.DisconnectedInGamePlayers.Select(x => x.Instance));
        players.Sort((x, y) => y.Score.CompareTo(x.Score));
        return new GameEndResultsPayload(players, isValid);
#endif
    }

#if STEREO
    private record class GameEndResultsPayload(List<GameLogic.Team> Teams, bool IsValid)
        : GameEndPayload(Teams)
#else
    private record class GameEndResultsPayload(List<GameLogic.Player> Players, bool IsValid)
        : GameEndPayload(Players)
#endif
    {
    }

#endif
    }
