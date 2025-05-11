using System.Net;
using System.Net.WebSockets;
using GameLogic;
using GameLogic.Networking;
using GameServer.System;
using Serilog;

namespace GameServer;

/// <summary>
/// Handles incoming player connection.
/// </summary>
/// <param name="context">The HTTP listener context.</param>
/// <param name="socket">The WebSocket connection.</param>
/// <param name="unknown">The unknown connection.</param>
/// <param name="game">The game instance.</param>
/// <param name="logger">The logger instance.</param>
internal sealed class PlayerConnectionHandler(
    HttpListenerContext context,
    WebSocket socket,
    UnknownConnection unknown,
    GameInstance game,
    ILogger logger)
{
    /// <summary>
    /// Handles the player connection request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync()
    {
        unknown.TargetType = "Player";

#if !STEREO
        string? nickname = context.Request.QueryString["nickname"]?.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(nickname))
        {
            await this.RejectAsync("MissingNickname");
            return;
        }
#endif

        if (!Enum.TryParse(context.Request.QueryString["playerType"], true, out PlayerType playerType))
        {
            await this.RejectAsync("InvalidPlayerType");
            return;
        }

#if STEREO
        string? teamName = context.Request.QueryString["teamName"];
        string? rawTankType = context.Request.QueryString["tankType"];

        if (string.IsNullOrWhiteSpace(teamName))
        {
            await this.RejectAsync("MissingTeamName");
            return;
        }

        if (!Enum.TryParse(rawTankType, ignoreCase: true, out TankType tankType))
        {
            await this.RejectAsync("InvalidTankType");
            return;
        }
#endif

#if DEBUG
        _ = bool.TryParse(context.Request.QueryString["quickJoin"], out bool quickJoin);
#else
        const bool quickJoin = false;
#endif

        Player player;
        PlayerConnection connection;

        lock (game)
        {
            if (!quickJoin && !game.Settings.SandboxMode && game.GameManager.IsInProgess)
            {
                _ = this.RejectAsync("GameInProgress");
                return;
            }

            if (!quickJoin && game.Players.Count() >= game.Settings.NumberOfPlayers)
            {
                _ = this.RejectAsync("GameFull");
                return;
            }

#if !STEREO
            if (this.NicknameExists(nickname))
            {
                if (quickJoin || game.Settings.SandboxMode)
                {
                    nickname = this.GenerateUniqueNickname(nickname);
                }
                else
                {
                    _ = this.RejectAsync("NicknameExists");
                    return;
                }
            }
#endif

#if STEREO
            if (game.Teams.Count() == 2 && game.Teams.All(t => t.Name != teamName))
            {
                _ = this.RejectAsync("TeamsFull");
                return;
            }

            if (game.Teams.FirstOrDefault(t => t.Name == teamName) is Team t &&
                t.Players.Any(p => p.Tank.Type == tankType))
            {
                _ = this.RejectAsync("TankTypeTaken");
                return;
            }
#endif

            var connectionData = new ConnectionData.Player(playerType, unknown.EnumSerialization)
            {
#if STEREO
                TeamName = teamName,
                TankType = tankType,
#else
                Nickname = nickname,
#endif
#if DEBUG
                QuickJoin = quickJoin,
#endif
            };

#if STEREO
            player = game.PlayerManager.CreatePlayer(connectionData, out Team team);
#else
            player = game.PlayerManager.CreatePlayer(connectionData);
#endif
            connection = new PlayerConnection(context, socket, connectionData, logger, player)
            {
#if STEREO
                Team = team,
#endif
            };

            game.AddConnection(connection);
        }

        await this.AcceptAsync(connection);
        _ = Task.Run(() => PacketListeningService.StartReceivingAsync(game, connection, game.PacketHandler, logger));

        if (quickJoin)
        {
            game.GameManager.StartGame();
            await game.LobbyManager.SendLobbyDataTo(connection);
        }

        if (game.GameManager.Status is GameStatus.InLobby)
        {
            _ = Task.Run(game.LobbyManager.SendLobbyDataToAll);
        }

        _ = Task.Run(() => PingHelper.Start(connection, game, logger));
    }

#if !STEREO
    private bool NicknameExists(string nickname)
    {
        return game.Players.Any(p => p.Instance.Nickname == nickname);
    }

    private string GenerateUniqueNickname(string baseName)
    {
        string newName = baseName;
        int counter = 1;
        while (this.NicknameExists(newName))
        {
            newName = $"{baseName}{counter++}";
        }

        return newName;
    }
#endif

    private async Task RejectAsync(string reason)
    {
        logger.Information("Connection rejected; {connection}; {reason}", unknown, reason);

        var payload = new ConnectionRejectedPayload(reason);
        var packet = new ResponsePacket(payload, logger);
        await packet.SendAsync(unknown);
        await unknown.CloseAsync(description: reason);
    }

    private async Task AcceptAsync(Connection connection)
    {
        var payload = new EmptyPayload { Type = PacketType.ConnectionAccepted };
        var packet = new ResponsePacket(payload, logger);
        await packet.SendAsync(connection);
    }
}
