using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Edomozh.Clock.Forms;
using Edomozh.Clock.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;

namespace Edomozh.Clock;

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
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
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
        _generatedIcon = CreateClockIcon();
        _trayIcon.Icon = _generatedIcon;
    }

    private static Icon CreateClockIcon()
    {
        const int size = 32;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        using var pen = new Pen(Color.White, 2);
        g.DrawEllipse(pen, 2, 2, size - 5, size - 5);

        int cx = size / 2, cy = size / 2;
        g.DrawLine(pen, cx, cy, cx - 6, cy - 8);  // Hour hand
        g.DrawLine(pen, cx, cy, cx + 6, cy - 10); // Minute hand

        return Icon.FromHandle(bitmap.GetHicon());
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
        _generatedIcon?.Dispose();
        _generatedIcon = null;

        _mainWindow?.Close();

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
