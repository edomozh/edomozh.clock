using System.Diagnostics;
using Microsoft.Win32;

namespace Edomozh.Clock.Services;

public class AutostartService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "edomozh.clock";

    public string? LastError { get; private set; }

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
                LastError = ex.Message;
                return false;
            }
        }
    }

    private static string? GetExecutablePath()
    {
        var path = Environment.ProcessPath;
        

        if (string.IsNullOrEmpty(path))
        {
            try
            {
                path = Process.GetCurrentProcess().MainModule?.FileName;
            }
            catch
            {
            }
        }

        if (!string.IsNullOrEmpty(path) && path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            path = System.IO.Path.ChangeExtension(path, ".exe");
        }

        return path;
    }

    public bool SetEnabled(bool enable)
    {
        LastError = null;
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null)
            {
                LastError = "Failed to open registry key for autostart";
                Debug.WriteLine(LastError);
                return false;
            }

            if (enable)
            {
                var exePath = GetExecutablePath();
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                    Debug.WriteLine($"Autostart enabled with path: {exePath}");
                    return true;
                }
                else
                {
                    LastError = "Failed to get executable path for autostart";
                    Debug.WriteLine(LastError);
                    return false;
                }
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
                Debug.WriteLine("Autostart disabled");
                return true;
            }
        }
        catch (Exception ex)
        {
            LastError = $"Failed to set autostart: {ex.Message}";
            Debug.WriteLine(LastError);
            return false;
        }
    }

    public bool SyncWithSettings(bool startWithWindows)
    {
        if (IsEnabled != startWithWindows)
        {
            return SetEnabled(startWithWindows);
        }
        return true;
    }
}
