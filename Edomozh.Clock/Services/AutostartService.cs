using System.Diagnostics;
using Microsoft.Win32;

namespace Edomozh.Clock.Services;

/// <summary>
/// Handles Windows autostart via Registry.
/// </summary>
public class AutostartService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "edomozh.clock";

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
                var value = key?.GetValue(AppName);
                return value != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to check autostart: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the executable path for the current process.
    /// Uses multiple methods for reliability across different deployment scenarios.
    /// </summary>
    private static string? GetExecutablePath()
    {
        // Try Environment.ProcessPath first (recommended for .NET 6+)
        var path = Environment.ProcessPath;
        
        // Fallback to MainModule.FileName for edge cases
        if (string.IsNullOrEmpty(path))
        {
            try
            {
                path = Process.GetCurrentProcess().MainModule?.FileName;
            }
            catch
            {
                // MainModule can throw in some scenarios
            }
        }

        // Ensure we have an exe path, not a dll path
        if (!string.IsNullOrEmpty(path) && path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            // In development, replace .dll with .exe
            path = System.IO.Path.ChangeExtension(path, ".exe");
        }

        return path;
    }

    /// <summary>
    /// Enables or disables autostart.
    /// </summary>
    public void SetEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null)
            {
                Debug.WriteLine("Failed to open registry key for autostart");
                return;
            }

            if (enable)
            {
                var exePath = GetExecutablePath();
                if (!string.IsNullOrEmpty(exePath))
                {
                    // Use quoted path to handle spaces in path
                    key.SetValue(AppName, $"\"{exePath}\"");
                    Debug.WriteLine($"Autostart enabled with path: {exePath}");
                }
                else
                {
                    Debug.WriteLine("Failed to get executable path for autostart");
                }
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
                Debug.WriteLine("Autostart disabled");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set autostart: {ex.Message}");
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
