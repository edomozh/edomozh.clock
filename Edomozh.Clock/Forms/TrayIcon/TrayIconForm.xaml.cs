using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Edomozh.Clock.Forms;
using Edomozh.Clock.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace Edomozh.Clock;

/// <summary>
/// Simple ICommand implementation for tray icon commands.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
#pragma warning disable CS0067 // Required by ICommand but unused
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}

/// <summary>
/// Application entry point with system tray management.
/// </summary>
public partial class TrayIconForm : Application
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const string MutexName = "edomozh.clock.SingleInstance";

    private Mutex? _mutex;
    private TaskbarIcon? _trayIcon;
    private MainForm? _mainWindow;
    private SettingsForm? _settingsWindow;
    private Icon? _generatedIcon;

    private readonly SettingsService _settingsService = new();
    private readonly AutostartService _autostartService = new();

    public static ICommand OpenSettingsCommand { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Single instance check
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            new InfoForm("Already Running", "Edomozh Clock is already running.").ShowDialog();
            Shutdown();
            return;
        }

        base.OnStartup(e);

        // Initialize command before loading resources
        OpenSettingsCommand = new RelayCommand(OpenSettings);

        // Load settings
        _settingsService.Load();

        // Create and show main clock window
        _mainWindow = new MainForm(_settingsService);
        _mainWindow.Show();

        // Initialize tray icon with generated icon
        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _generatedIcon = CreateClockIcon();
        _trayIcon.Icon = _generatedIcon;
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

        _settingsWindow = new SettingsForm(_settingsService, _autostartService);
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
