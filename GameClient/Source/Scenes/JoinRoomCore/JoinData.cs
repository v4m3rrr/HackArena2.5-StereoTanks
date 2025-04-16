using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameClient.Scenes.JoinRoomCore;

/// <summary>
/// Represents the connection data.
/// </summary>
internal static class JoinData
{
    /// <summary>
    /// The default address of the server.
    /// </summary>
    public const string DefaultAddress = "localhost:5000";

    private const string FilePath = "connection.json";

    /// <summary>
    /// Gets or sets the nickname of the player.
    /// </summary>
    public static string? Nickname { get; set; }

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

        Nickname = data.Nickname;
        Address = data.Address;
    }

    /// <summary>
    /// Saves the connection data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Save()
    {
        var data = new Data(Nickname, Address);
        string json = JsonSerializer.Serialize(data);

        await GameClientCore.InvokeOnMainThreadAsync(() => File.WriteAllText(FilePath, json));
    }

    private record struct Data(string? Nickname, string? Address);
}
