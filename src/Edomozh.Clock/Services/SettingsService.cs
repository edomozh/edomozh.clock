using System.IO;
using System.Text.Json;
using Edomozh.Clock.Models;

namespace Edomozh.Clock.Services;

/// <summary>
/// Handles loading and saving settings to %APPDATA%\edomozh.clock\settings.config
/// </summary>
public class SettingsService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "edomozh.clock");

    private static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.config");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private ClockSettings _settings = ClockSettings.CreateDefault();

    /// <summary>
    /// Current settings instance.
    /// </summary>
    public ClockSettings Settings => _settings;

    /// <summary>
    /// Event raised when settings are changed and saved.
    /// </summary>
    public event EventHandler? SettingsChanged;

    /// <summary>
    /// Loads settings from disk. Creates default if file doesn't exist.
    /// </summary>
    public ClockSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                _settings = JsonSerializer.Deserialize<ClockSettings>(json, JsonOptions) 
                    ?? ClockSettings.CreateDefault();
            }
            else
            {
                _settings = ClockSettings.CreateDefault();
                // Add default preset
                _settings.Presets.Add(ThemePreset.CreateDefault());
                Save();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            _settings = ClockSettings.CreateDefault();
        }

        return _settings;
    }

    /// <summary>
    /// Saves current settings to disk.
    /// </summary>
    public void Save()
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(AppDataFolder);

            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates settings and saves to disk.
    /// </summary>
    public void Update(Action<ClockSettings> updateAction)
    {
        updateAction(_settings);
        Save();
    }

    /// <summary>
    /// Resets settings to defaults and saves.
    /// </summary>
    public void Reset()
    {
        _settings = ClockSettings.CreateDefault();
        _settings.Presets.Add(ThemePreset.CreateDefault());
        Save();
    }

    /// <summary>
    /// Gets the settings folder path for display/debugging.
    /// </summary>
    public static string GetSettingsFolder() => AppDataFolder;

    /// <summary>
    /// Gets the settings file path for display/debugging.
    /// </summary>
    public static string GetSettingsFilePath() => SettingsFilePath;
}
