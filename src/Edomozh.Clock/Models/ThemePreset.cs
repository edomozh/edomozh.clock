namespace Edomozh.Clock.Models;

/// <summary>
/// Represents a saved appearance theme preset.
/// </summary>
public class ThemePreset
{
    public string Name { get; set; } = "Default";
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 48;
    public string TextColor { get; set; } = "#FFFFFF";
    public double TextOpacity { get; set; } = 1.0;
    public double BackgroundOpacity { get; set; } = 0.0;
    public string BackgroundColor { get; set; } = "#000000";

    public ThemePreset Clone()
    {
        return new ThemePreset
        {
            Name = Name,
            FontFamily = FontFamily,
            FontSize = FontSize,
            TextColor = TextColor,
            TextOpacity = TextOpacity,
            BackgroundOpacity = BackgroundOpacity,
            BackgroundColor = BackgroundColor
        };
    }

    public static ThemePreset CreateDefault() => new();
}
