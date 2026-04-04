using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Edomozh.Clock.Helpers;
using Edomozh.Clock.Services;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace Edomozh.Clock.Forms;

public partial class MainForm : Window
{
    private readonly DispatcherTimer _timer;
    private readonly SettingsService _settingsService;
    private bool _isEditMode;

    public MainForm(SettingsService settingsService)
    {
        InitializeComponent();

        _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += Timer_Tick;

        _settingsService = settingsService;
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            _isEditMode = value;
            WindowHelper.SetClickThrough(this, !value);

            ClockBorder.BorderThickness = value ? new Thickness(2) : new Thickness(0);
            ClockBorder.BorderBrush = value ? new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)) : null;

            Cursor = value ? System.Windows.Input.Cursors.SizeAll : System.Windows.Input.Cursors.Arrow;

            if (!value)
            {
                SavePosition();
            }
        }
    }

    public void SavePosition()
    {
        _settingsService.Update(s =>
        {
            s.PositionX = Left;
            s.PositionY = Top;
        });
        ApplyPosition();
    }

    public void ResetPosition()
    {
        _settingsService.Update(s =>
        {
            s.PositionX = 20;
            s.PositionY = 20;
        });
        ApplyPosition();
    }

    private void ApplyPosition()
    {
        Left = _settingsService.Settings.PositionX;
        Top = _settingsService.Settings.PositionY;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WindowHelper.SetOverlayMode(this);

        ApplySettings();
        ApplyPosition();

        UpdateTime();
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();

        if (_settingsService.Settings.AlwaysOnTop)
        {
            WindowHelper.SetTopmost(this, true);
        }
    }

    private void UpdateTime()
    {
        var settings = _settingsService.Settings;
        var now = DateTime.Now;

        TimeText.Text = now.ToString(settings.GetTimeFormat());

        var dateFormat = settings.GetDateFormat();
        if (dateFormat != null)
        {
            DateText.Text = now.ToString(dateFormat);
            DateText.Visibility = Visibility.Visible;
        }
        else
        {
            DateText.Visibility = Visibility.Collapsed;
        }
    }

    private void ApplySettings()
    {
        var settings = _settingsService.Settings;

        Topmost = settings.AlwaysOnTop;
        if (IsLoaded)
        {
            WindowHelper.SetTopmost(this, settings.AlwaysOnTop);
        }

        TimeText.FontFamily = new FontFamily(settings.FontFamily);
        TimeText.FontSize = settings.FontSize;
        DateText.FontFamily = new FontFamily(settings.FontFamily);
        DateText.FontSize = settings.FontSize * 0.35;

        try
        {
            var textColor = (Color)ColorConverter.ConvertFromString(settings.TextColor);
            TextBrush.Color = textColor;
            DateTextBrush.Color = textColor;
            TextBrush.Opacity = settings.TextOpacity;
            DateTextBrush.Opacity = settings.TextOpacity * 0.8;

            var bgColor = (Color)ColorConverter.ConvertFromString(settings.BackgroundColor);
            BackgroundBrush.Color = bgColor;
            BackgroundBrush.Opacity = settings.BackgroundOpacity;
        }
        catch
        {
            TextBrush.Color = Colors.White;
            BackgroundBrush.Color = Colors.Black;
        }

        UpdateTime();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(ApplySettings);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_isEditMode && e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    public void RefreshFromSettings()
    {
        ApplySettings();
    }

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();
        _settingsService.SettingsChanged -= OnSettingsChanged;
        base.OnClosed(e);
    }
}
