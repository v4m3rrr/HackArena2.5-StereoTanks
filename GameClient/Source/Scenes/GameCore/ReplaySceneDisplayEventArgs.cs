using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GameLogic.Networking;
using MonoRivUI;
using Newtonsoft.Json.Linq;

namespace GameClient.Scenes.GameCore;

/// <summary>
/// Represents the game replay scene display event arguments.
/// </summary>
/// <param name="absPath">The absolute path of the replay file to display.</param>
internal class ReplaySceneDisplayEventArgs(string absPath)
    : SceneDisplayEventArgs(false), IDisposable
{
    private bool disposed;

    /// <summary>
    /// Finalizes an instance of the <see cref="ReplaySceneDisplayEventArgs"/> class.
    /// </summary>
    ~ReplaySceneDisplayEventArgs()
    {
        this.Dispose(disposing: false);
    }

    /// <summary>
    /// Gets the absolute path of the replay file to display.
    /// </summary>
    public string AbsPath { get; } = absPath;

    /// <summary>
    /// Gets the data of the replay file.
    /// </summary>
    public JObject Data { get; private set; } = default!;

    /// <summary>
    /// Gets the lobby data.
    /// </summary>
    public LobbyDataPayload LobbyData { get; private set; } = default!;

    /// <summary>
    /// Gets the game states.
    /// </summary>
    public GameStatePayload[] GameStates { get; private set; } = default!;

    /// <summary>
    /// Gets the game end.
    /// </summary>
    public GameEndPayload GameEnd { get; private set; } = default!;

#if HACKATHON

    /// <summary>
    /// Gets a value indicating whether to enable show mode.
    /// </summary>
    public bool ShowMode { get; init; }

    /// <summary>
    /// Gets the match results.
    /// </summary>
    public JObject? MatchResults { get; private set; }

#endif

    /// <summary>
    /// Loads the data of the replay file.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadData()
    {
        this.Data = JObject.Parse(await File.ReadAllTextAsync(this.AbsPath));

        var converters = LobbyDataPayload.GetConverters();
        var serializer = PacketSerializer.GetSerializer(converters);
        this.LobbyData = this.Data["lobbyData"]!.ToObject<LobbyDataPayload>(serializer)!;

        var context = new GameSerializationContext.Spectator();
        converters = GameStatePayload.GetConverters(context);
        serializer = PacketSerializer.GetSerializer(converters);
        this.GameStates = this.Data["gameStates"]!.ToObject<GameStatePayload[]>(serializer)!;

        converters = GameEndPayload.GetConverters();
        serializer = PacketSerializer.GetSerializer(converters);
        this.GameEnd = this.Data["gameEnd"]!.ToObject<GameEndPayload>(serializer)!;

#if HACKATHON

        if (this.ShowMode)
        {
            try
            {
                var replaysDirectory = Path.GetDirectoryName(this.AbsPath)!;

                var replayFilename = Path.GetFileName(this.AbsPath);
                var replayFilenameWithoutExtension = Path.GetFileNameWithoutExtension(replayFilename);
                var replayFilenameExtension = Path.GetExtension(replayFilename);

                var matchResultsFilename = $"{replayFilenameWithoutExtension}_match_results{replayFilenameExtension}";
                var matchResultsPath = PathUtils.GetAbsolutePath($"{replaysDirectory}/{matchResultsFilename}");

                this.MatchResults = JObject.Parse(await File.ReadAllTextAsync(matchResultsPath));
            }
            catch (IOException ex)
            {
                DebugConsole.ThrowError("Failed to load match results.");
                DebugConsole.ThrowError(ex);
            }
        }

#endif

    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// A value indicating whether to dispose the managed resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            this.Data = default!;
            this.LobbyData = default!;
            this.GameStates = default!;
            this.GameEnd = default!;

#if HACKATHON
            this.MatchResults = default!;
#endif

            this.disposed = true;
        }
    }
}
