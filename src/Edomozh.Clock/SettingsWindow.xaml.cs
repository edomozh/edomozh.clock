using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Edomozh.Clock.Models;
using Edomozh.Clock.Services;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace Edomozh.Clock;

/// <summary>
/// Settings dialog window.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly AutostartService _autostartService;
    private bool _isLoading;

    public SettingsWindow(SettingsService settingsService, AutostartService autostartService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        _autostartService = autostartService;

        LoadFonts();
        LoadFromSettings();
        LoadPresets();
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

            // Time format
            Use24HourCheckBox.IsChecked = settings.Use24HourFormat;
            ShowSecondsCheckBox.IsChecked = settings.ShowSeconds;
            ShowDateCheckBox.IsChecked = settings.ShowDate;
            CustomFormatTextBox.Text = settings.CustomTimeFormat ?? "";

            // Appearance
            FontComboBox.SelectedItem = settings.FontFamily;
            FontSizeSlider.Value = settings.FontSize;
            FontSizeLabel.Text = ((int)settings.FontSize).ToString();
            TextColorTextBox.Text = settings.TextColor;
            UpdateColorPreview();
            TextOpacitySlider.Value = settings.TextOpacity;
            TextOpacityLabel.Text = $"{(int)(settings.TextOpacity * 100)}%";
            BgOpacitySlider.Value = settings.BackgroundOpacity;
            BgOpacityLabel.Text = $"{(int)(settings.BackgroundOpacity * 100)}%";

            // General
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

        // Time format
        settings.Use24HourFormat = Use24HourCheckBox.IsChecked ?? true;
        settings.ShowSeconds = ShowSecondsCheckBox.IsChecked ?? true;
        settings.ShowDate = ShowDateCheckBox.IsChecked ?? false;
        settings.CustomTimeFormat = string.IsNullOrWhiteSpace(CustomFormatTextBox.Text) 
            ? null 
            : CustomFormatTextBox.Text;

        // Appearance
        settings.FontFamily = FontComboBox.SelectedItem?.ToString() ?? "Segoe UI";
        settings.FontSize = FontSizeSlider.Value;
        settings.TextColor = TextColorTextBox.Text;
        settings.TextOpacity = TextOpacitySlider.Value;
        settings.BackgroundOpacity = BgOpacitySlider.Value;

        // General
        settings.AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? true;
        settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;

        // Save and sync autostart
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
        var dialog = new InputDialog("Save Preset", "Enter preset name:");
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
        {
            var name = dialog.InputText.Trim();
            var settings = _settingsService.Settings;

            // Remove existing preset with same name
            settings.Presets.RemoveAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            // Create new preset from current UI values
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

        if (MessageBox.Show($"Delete preset '{presetName}'?", "Confirm Delete", 
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        SaveToSettings();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

/// <summary>
/// Simple input dialog for preset names.
/// </summary>
public class InputDialog : System.Windows.Window
{
    private readonly System.Windows.Controls.TextBox _textBox;

    public string InputText => _textBox.Text;

    public InputDialog(string title, string prompt)
    {
        Title = title;
        Width = 400;
        Height = 180;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;

        var grid = new Grid { Margin = new Thickness(16) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 8) };
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        _textBox = new System.Windows.Controls.TextBox { Height = 28 };
        Grid.SetRow(_textBox, 1);
        grid.Children.Add(_textBox);

        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0)
        };
        Grid.SetRow(buttonPanel, 2);

        var okButton = new Button { Content = "OK", Width = 70, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
        okButton.Click += (s, e) => { DialogResult = true; Close(); };
        buttonPanel.Children.Add(okButton);

        var cancelButton = new Button { Content = "Cancel", Width = 70, IsCancel = true };
        cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
        buttonPanel.Children.Add(cancelButton);

        grid.Children.Add(buttonPanel);
        Content = grid;

        Loaded += (s, e) => _textBox.Focus();
    }
}
