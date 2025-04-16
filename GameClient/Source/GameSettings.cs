using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoRivUI;

namespace GameClient;

/// <summary>
/// Represents the game settings.
/// </summary>
internal static class GameSettings
{
    private const string SettingsFilePath = "settings.json";

    private static SettingsData data;

    /// <summary>
    /// Occurs when the language is changing.
    /// </summary>
    public static event EventHandler? LanguageChanging;

    /// <summary>
    /// Occurs when the language has changed.
    /// </summary>
    public static event EventHandler? LanguageChanged;

    /// <summary>
    /// Occurs when the resolution is changing.
    /// </summary>
    public static event EventHandler? ResolutionChanging;

    /// <summary>
    /// Occurs when the resolution has changed.
    /// </summary>
    public static event EventHandler? ResolutionChanged;

    /// <summary>
    /// Occurs when the screen type is changing.
    /// </summary>
    public static event EventHandler? ScreenTypeChanging;

    /// <summary>
    /// Occurs when the screen type has changed.
    /// </summary>
    public static event EventHandler? ScreenTypeChanged;

    /// <summary>
    /// Gets or sets the language of the game.
    /// </summary>
    public static Language Language
    {
        get => data.Language;
        set
        {
            if (data.Language == value)
            {
                return;
            }

            LanguageChanging?.Invoke(null, EventArgs.Empty);
            data.Language = value;
            LanguageChanged?.Invoke(null, EventArgs.Empty);
            ILocalizable.RefreshAll();
        }
    }

    /// <summary>
    /// Sets the resolution of the game.
    /// </summary>
    /// <param name="width">The width of the resolution.</param>
    /// <param name="height">The height of the resolution.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Width and height must be greater than 0.</exception>
    public static async Task SetResolution(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        if (ScreenController.Width == width && ScreenController.Height == height)
        {
            return;
        }

        ResolutionChanging?.Invoke(null, EventArgs.Empty);
        data.ResolutionWidth = width;
        data.ResolutionHeight = height;
        ScreenController.Change(width, height);

        await GameClientCore.InvokeOnMainThreadAsync(ScreenController.ApplyChanges);

        ResolutionChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the screen type of the game.
    /// </summary>
    /// <param name="screenType">The screen type to set.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SetScreenType(ScreenType screenType)
    {
        if (ScreenController.ScreenType == screenType)
        {
            return;
        }

        ScreenTypeChanging?.Invoke(null, EventArgs.Empty);
        data.ScreenType = screenType;
        ScreenController.Change(screenType: screenType);

        await GameClientCore.InvokeOnMainThreadAsync(ScreenController.ApplyChanges);

        ScreenTypeChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Saves the settings.
    /// </summary>
    public static void SaveSettings()
    {
        string json = JsonSerializer.Serialize(data);
        File.WriteAllText(SettingsFilePath, json);
    }

    /// <summary>
    /// Loads settings from file.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task LoadSettings()
    {
        if (!File.Exists(SettingsFilePath))
        {
            SetDefaultSettings();
            SaveSettings();
            return;
        }

        try
        {
            string json = await GameClientCore.InvokeOnMainThreadAsync(() => File.ReadAllText(SettingsFilePath));
            var settings = JsonSerializer.Deserialize<SettingsData>(json);

            Language = settings.Language;
            await SetResolution(settings.ResolutionWidth, settings.ResolutionHeight);
            await SetScreenType(settings.ScreenType);
        }
        catch (Exception ex)
        {
            DebugConsole.ThrowError($"Failed to load settings. Default settings will be used.");
            DebugConsole.ThrowError(ex);
            SetDefaultSettings();
        }
    }

    private static async void SetDefaultSettings()
    {
        Language = Language.English;
        await SetResolution(1366, 768);
        await SetScreenType(ScreenType.Windowed);
    }

    private record struct SettingsData(
        Language Language,
        int ResolutionWidth,
        int ResolutionHeight,
        ScreenType ScreenType);
}
