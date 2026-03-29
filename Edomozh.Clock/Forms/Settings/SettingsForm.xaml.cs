using System.Windows;
using System.Windows.Media;
using Edomozh.Clock.Forms;
using Edomozh.Clock.Models;
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

        CustomFormatTextBox.TextChanged += (s, e) => UpdateCustomFormatPlaceholder();

        LoadFonts();
        LoadFromSettings();
        LoadPresets();
        
        UpdateCustomFormatPlaceholder();
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
        FontComboBox.SelectedItem = "Segoe UI";
    }

    private void LoadFromSettings()
    {
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
            _isLoading = false;
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

    private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading || FontSizeLabel == null) return;
        FontSizeLabel.Text = ((int)e.NewValue).ToString();
    }

    private void TextOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading || TextOpacityLabel == null) return;
        TextOpacityLabel.Text = $"{(int)(e.NewValue * 100)}%";
    }

    private void BgOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isLoading || BgOpacityLabel == null) return;
        BgOpacityLabel.Text = $"{(int)(e.NewValue * 100)}%";
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
            TextColorTextBox.Text = preset.TextColor;
            UpdateColorPreview();
            TextOpacitySlider.Value = preset.TextOpacity;
            BgOpacitySlider.Value = preset.BackgroundOpacity;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SavePresetButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputForm("Save Preset", "Enter preset name:");
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
                BackgroundOpacity = BgOpacitySlider.Value
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

        var dialog = new ConfirmForm("Confirm Delete", $"Delete preset {presetName}?");
        if (dialog.ShowDialog() == true)
        {
            _settingsService.Settings.Presets.RemoveAll(p => p.Name == presetName);
            _settingsService.Save();
            LoadPresets();
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        SaveToSettings();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
