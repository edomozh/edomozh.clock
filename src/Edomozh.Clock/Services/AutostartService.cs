using Microsoft.Win32;

namespace Edomozh.Clock.Services;

/// <summary>
/// Handles Windows autostart via Registry.
/// </summary>
public class AutostartService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Edomozh.Clock";

    /// <summary>
    /// Gets whether the application is configured to start with Windows.
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Enables or disables autostart.
    /// </summary>
    public void SetEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set autostart: {ex.Message}");
        }
    }

    /// <summary>
    /// Synchronizes autostart state with settings.
    /// </summary>
    public void SyncWithSettings(bool startWithWindows)
    {
        if (IsEnabled != startWithWindows)
        {
            SetEnabled(startWithWindows);
        }
    }
}
