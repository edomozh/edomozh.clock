using System;
using System.IO;
using System.Text.Json;
using Edomozh.Clock.Models;

namespace Edomozh.Clock.Services;

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

    public ClockSettings Settings => _settings;

    public event EventHandler? SettingsChanged;

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

    public void Save()
    {
        try
        {
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

    public void Update(Action<ClockSettings> updateAction)
    {
        updateAction(_settings);
        Save();
    }

    public void Reset()
    {
        _settings = ClockSettings.CreateDefault();
        _settings.Presets.Add(ThemePreset.CreateDefault());
        Save();
    }

    public static string GetSettingsFolder() => AppDataFolder;

    public static string GetSettingsFilePath() => SettingsFilePath;
}
