using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GameLogic;

namespace GameClient.Scenes.SinglePlayerCore;

/// <summary>
/// Represents the connection data.
/// </summary>
internal static class SinglePlayerData
{
    /// <summary>
    /// The default address of the server.
    /// </summary>
    public const string DefaultAddress = "localhost:5000";

    private const string FilePath = "connection.json";

    public const string DefaultTeamName = "Player";
    public const TankType DefaultTankType = TankType.Light;
    public const Difficulty DefaultDifficulty = Difficulty.Easy;

#if STEREO

    /// <summary>
    /// Gets or sets the team name of the player.
    /// </summary>
    public static string? TeamName { get; set; }

    /// <summary>
    /// Gets or sets the tank type of the player.
    /// </summary>
    public static TankType TankType { get; set; }

    /// <summary>
    /// Gets or sets the difficulty.
    /// </summary>
    public static Difficulty Difficulty { get; set; }

#else

    /// <summary>
    /// Gets or sets the nickname of the player.
    /// </summary>
    public static string? Nickname { get; set; }

#endif

    /// <summary>
    /// Gets or sets the address of the server.
    /// </summary>
    public static string? Address { get; set; }

    /// <summary>
    /// Loads the connection data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Load()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }

        string json = await GameClientCore.InvokeOnMainThreadAsync(() => File.ReadAllText(FilePath));
        var data = JsonSerializer.Deserialize<Data>(json);

#if STEREO
        TeamName = data.TeamName;
        TankType = data.TankType;
        Difficulty = data.Difficulty;
#else
        Nickname = data.Nickname;
#endif

        Address = data.Address;
    }

    /// <summary>
    /// Saves the connection data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Save()
    {
#if STEREO
        var data = new Data(TeamName, TankType, Difficulty, Address);
#else
        var data = new Data(Nickname, Address);
#endif

        string json = JsonSerializer.Serialize(data);

        await GameClientCore.InvokeOnMainThreadAsync(() => File.WriteAllText(FilePath, json));
    }

#if STEREO
    private record struct Data(string? TeamName, TankType TankType, Difficulty Difficulty, string? Address);
#else
    private record struct Data(string? Nickname, string? Address);
#endif
}
