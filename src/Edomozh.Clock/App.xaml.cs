using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Edomozh.Clock.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Edomozh.Clock;

/// <summary>
/// Application entry point with system tray management.
/// </summary>
public partial class App : Application
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const string MutexName = "Edomozh.Clock.SingleInstance";
    
    private Mutex? _mutex;
    private TaskbarIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private SettingsWindow? _settingsWindow;
    private Icon? _generatedIcon;
    
    private readonly SettingsService _settingsService = new();
    private readonly AutostartService _autostartService = new();

    public static RoutedCommand OpenSettingsCommand { get; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        // Single instance check
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Edomozh Clock is already running.", "Already Running", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        // Load settings
        _settingsService.Load();

        // Create and show main clock window
        _mainWindow = new MainWindow(_settingsService);
        _mainWindow.Show();

        // Initialize tray icon with generated icon
        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _generatedIcon = CreateClockIcon();
        _trayIcon.Icon = _generatedIcon;
        
        // Wire up double-click command
        _mainWindow.CommandBindings.Add(new CommandBinding(OpenSettingsCommand, (s, args) => OpenSettings()));
    }

    /// <summary>
    /// Creates a simple clock icon programmatically.
    /// </summary>
    private static Icon CreateClockIcon()
    {
        const int size = 32;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        
        // Draw clock circle
        using var pen = new Pen(Color.White, 2);
        g.DrawEllipse(pen, 2, 2, size - 5, size - 5);
        
        // Draw clock hands (pointing to ~10:10)
        int cx = size / 2, cy = size / 2;
        g.DrawLine(pen, cx, cy, cx - 6, cy - 8);  // Hour hand
        g.DrawLine(pen, cx, cy, cx + 6, cy - 10); // Minute hand
        
        return Icon.FromHandle(bitmap.GetHicon());
    }

    private void TrayIcon_PreviewTrayContextMenuOpen(object sender, RoutedEventArgs e)
    {
        // This fixes the "context menu closes immediately" bug on first click.
        // Windows requires a foreground window before showing a popup from the notification area.
        // We create a temporary invisible window to get foreground, then close it.
        var helper = new System.Windows.Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            AllowsTransparency = true,
            Background = System.Windows.Media.Brushes.Transparent,
            Topmost = true
        };
        helper.Show();
        SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(helper).Handle);
        helper.Close();
    }

    private void EditModeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_mainWindow == null) return;
        
        var menuItem = sender as System.Windows.Controls.MenuItem;
        if (menuItem != null)
        {
            _mainWindow.IsEditMode = menuItem.IsChecked;
        }
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        OpenSettings();
    }

    private void OpenSettings()
    {
        // Don't open multiple settings windows
        if (_settingsWindow != null && _settingsWindow.IsVisible)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_settingsService, _autostartService);
        _settingsWindow.ShowDialog();
        _settingsWindow = null;
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ExitApplication();
    }

    private void ExitApplication()
    {
        // Save settings
        _settingsService.Save();

        // Dispose tray icon and generated icon
        _trayIcon?.Dispose();
        _trayIcon = null;
        _generatedIcon?.Dispose();
        _generatedIcon = null;

        // Close main window
        _mainWindow?.Close();

        // Release mutex
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _generatedIcon?.Dispose();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
