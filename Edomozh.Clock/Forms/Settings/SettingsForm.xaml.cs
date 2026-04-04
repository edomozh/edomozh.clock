using System.Windows;
using System.Windows.Media;
using Edomozh.Clock.Forms;
using Edomozh.Clock.Models;
using Edomozh.Clock.Resources;
using Edomozh.Clock.Services;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Brushes = System.Windows.Media.Brushes;

namespace Edomozh.Clock.Forms;

public partial class SettingsForm : Window
{
    private readonly SettingsService _settingsService;
    private readonly AutostartService _autostartService;
    private bool _isLoading;

    public SettingsForm(SettingsService settingsService, AutostartService autostartService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        _autostartService = autostartService;
        Icon = AppResources.ClockIconImageSource;

        _isLoading = true;

        LoadFonts();
        LoadFromSettings();
        LoadPresets();
        
        UpdateCustomFormatPlaceholder();

        CustomFormatTextBox.TextChanged += (s, e) => {
            UpdateCustomFormatPlaceholder();
            ApplyChanges();
        };
        TextColorTextBox.TextChanged += (s, e) => {
            UpdateColorPreview();
            ApplyChanges();
        };

        Use24HourCheckBox.Checked += (s, e) => ApplyChanges();
        Use24HourCheckBox.Unchecked += (s, e) => ApplyChanges();
        ShowSecondsCheckBox.Checked += (s, e) => ApplyChanges();
        ShowSecondsCheckBox.Unchecked += (s, e) => ApplyChanges();
        ShowDateCheckBox.Checked += (s, e) => ApplyChanges();
        ShowDateCheckBox.Unchecked += (s, e) => ApplyChanges();
        AlwaysOnTopCheckBox.Checked += (s, e) => ApplyChanges();
        AlwaysOnTopCheckBox.Unchecked += (s, e) => ApplyChanges();
        StartWithWindowsCheckBox.Checked += (s, e) => ApplyChanges();
        StartWithWindowsCheckBox.Unchecked += (s, e) => ApplyChanges();

        FontComboBox.SelectionChanged += (s, e) => ApplyChanges();

        _isLoading = false;
    }

    private void UpdateCustomFormatPlaceholder()
    {
        CustomFormatPlaceholder.Visibility = string.IsNullOrEmpty(CustomFormatTextBox.Text) 
            ? Visibility.Visible 
            : Visibility.Collapsed;
    }

    private void LoadFonts()
    {
        var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
        FontComboBox.ItemsSource = fonts.Select(f => f.Source);
    }

    private void LoadFromSettings()
    {
        var wasLoading = _isLoading;
        _isLoading = true;
        try
        {
            var settings = _settingsService.Settings;

            Use24HourCheckBox.IsChecked = settings.Use24HourFormat;
            ShowSecondsCheckBox.IsChecked = settings.ShowSeconds;
            ShowDateCheckBox.IsChecked = settings.ShowDate;
            CustomFormatTextBox.Text = settings.CustomTimeFormat ?? "";

            FontComboBox.SelectedItem = settings.FontFamily;
            FontSizeSlider.Value = settings.FontSize;
            FontSizeLabel.Text = ((int)settings.FontSize).ToString();
            TextColorTextBox.Text = settings.TextColor;
            UpdateColorPreview();
            TextOpacitySlider.Value = settings.TextOpacity;
            TextOpacityLabel.Text = $"{(int)(settings.TextOpacity * 100)}%";
            BgOpacitySlider.Value = settings.BackgroundOpacity;
            BgOpacityLabel.Text = $"{(int)(settings.BackgroundOpacity * 100)}%";

            AlwaysOnTopCheckBox.IsChecked = settings.AlwaysOnTop;
            StartWithWindowsCheckBox.IsChecked = _autostartService.IsEnabled;
        }
        finally
        {
            _isLoading = wasLoading;
        }
    }

    private void LoadPresets()
    {
        var presets = _settingsService.Settings.Presets;
        PresetsComboBox.ItemsSource = presets.Select(p => p.Name).ToList();
        if (presets.Count > 0)
        {
            PresetsComboBox.SelectedIndex = 0;
        }
    }

    private void SaveToSettings()
    {
        var settings = _settingsService.Settings;

        settings.Use24HourFormat = Use24HourCheckBox.IsChecked ?? true;
        settings.ShowSeconds = ShowSecondsCheckBox.IsChecked ?? true;
        settings.ShowDate = ShowDateCheckBox.IsChecked ?? false;
        settings.CustomTimeFormat = string.IsNullOrWhiteSpace(CustomFormatTextBox.Text)
            ? null 
            : CustomFormatTextBox.Text;

        settings.FontFamily = FontComboBox.SelectedItem?.ToString() ?? "Segoe UI";
        settings.FontSize = FontSizeSlider.Value;
        settings.TextColor = TextColorTextBox.Text;
        settings.TextOpacity = TextOpacitySlider.Value;
        settings.BackgroundOpacity = BgOpacitySlider.Value;

        settings.AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? true;
        settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

        _settingsService.Save();
        _autostartService.SyncWithSettings(settings.StartWithWindows);
    }

    private void UpdateColorPreview()
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(TextColorTextBox.Text);
            TextColorPreview.Fill = new SolidColorBrush(color);
        }
        catch
        {
            TextColorPreview.Fill = Brushes.White;
        }
    }

    private void ApplyChanges()
    {
        if (_isLoading) return;
        SaveToSettings();
    }

    private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading || FontSizeLabel == null) return;
        FontSizeLabel.Text = ((int)e.NewValue).ToString();
        ApplyChanges();
    }

    private void TextOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading || TextOpacityLabel == null) return;
        TextOpacityLabel.Text = $"{(int)(e.NewValue * 100)}%";
        ApplyChanges();
    }

    private void BgOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading || BgOpacityLabel == null) return;
        BgOpacityLabel.Text = $"{(int)(e.NewValue * 100)}%";
        ApplyChanges();
    }

    private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
    {
        var presetName = PresetsComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(presetName)) return;

        var preset = _settingsService.Settings.Presets.FirstOrDefault(p => p.Name == presetName);
        if (preset == null) return;

        _isLoading = true;
        try
        {
            FontComboBox.SelectedItem = preset.FontFamily;
            FontSizeSlider.Value = preset.FontSize;
            FontSizeLabel.Text = ((int)preset.FontSize).ToString();
            TextColorTextBox.Text = preset.TextColor;
            UpdateColorPreview();
            TextOpacitySlider.Value = preset.TextOpacity;
            TextOpacityLabel.Text = $"{(int)(preset.TextOpacity * 100)}%";
            BgOpacitySlider.Value = preset.BackgroundOpacity;
            BgOpacityLabel.Text = $"{(int)(preset.BackgroundOpacity * 100)}%";
        }
        finally
        {
            _isLoading = false;
        }
        SaveToSettings();
    }

    private void SavePresetButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputForm("Save Preset", "Enter preset name:") { Owner = this };
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
        {
            var name = dialog.InputText.Trim();
            var settings = _settingsService.Settings;

            settings.Presets.RemoveAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            var preset = new ThemePreset
            {
                Name = name,
                FontFamily = FontComboBox.SelectedItem?.ToString() ?? "Segoe UI",
                FontSize = FontSizeSlider.Value,
                TextColor = TextColorTextBox.Text,
                TextOpacity = TextOpacitySlider.Value,
                BackgroundOpacity = BgOpacitySlider.Value,
                BackgroundColor = settings.BackgroundColor
            };

            settings.Presets.Add(preset);
            _settingsService.Save();
            LoadPresets();
            PresetsComboBox.SelectedItem = name;
        }
    }

    private void DeletePresetButton_Click(object sender, RoutedEventArgs e)
    {
        var presetName = PresetsComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(presetName)) return;

        var dialog = new ConfirmForm("Confirm Delete", $"Delete preset {presetName}?") { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _settingsService.Settings.Presets.RemoveAll(p => p.Name == presetName);
            _settingsService.Save();
            LoadPresets();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
