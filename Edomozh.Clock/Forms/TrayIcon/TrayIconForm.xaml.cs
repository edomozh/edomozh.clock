using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Edomozh.Clock.Forms;
using Edomozh.Clock.Infrastructure;
using Edomozh.Clock.Resources;
using Edomozh.Clock.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace Edomozh.Clock;

public partial class TrayIconForm : Application
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const string MutexName = "edomozh.clock.SingleInstance";

    private Mutex? _mutex;
    private bool _mutexOwned;
    private TaskbarIcon? _trayIcon;
    private MainForm? _mainWindow;
    private SettingsForm? _settingsWindow;

    private readonly SettingsService _settingsService = new();
    private readonly AutostartService _autostartService = new();

    public static ICommand OpenSettingsCommand { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, MutexName, out _mutexOwned);
        if (!_mutexOwned)
        {
            new InfoForm("Already Running", "Edomozh Clock is already running.").ShowDialog();
            Shutdown();
            return;
        }

        base.OnStartup(e);

        OpenSettingsCommand = new RelayCommand(OpenSettings);

        _settingsService.Load();

        _mainWindow = new MainForm(_settingsService);
        _mainWindow.Show();

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _trayIcon.Icon = AppResources.ClockIcon;
    }

    private void TrayIcon_PreviewTrayContextMenuOpen(object sender, RoutedEventArgs e)
    {
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

    private void ResetPositionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow?.ResetPosition();
    }

    private void OpenSettings()
    {
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
        _settingsService.Save();

        _trayIcon?.Dispose();
        _trayIcon = null;

        _mainWindow?.Close();

        if (_mutexOwned && _mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutexOwned = false;
        }
        _mutex?.Dispose();
        _mutex = null;

        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        if (_mutexOwned && _mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutexOwned = false;
        }
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
