namespace Edomozh.Clock.Models;

public class ClockSettings
{
    public bool Use24HourFormat { get; set; } = true;
    public bool ShowSeconds { get; set; } = true;
    public bool ShowDate { get; set; } = false;
    public string? CustomTimeFormat { get; set; }
    public string? CustomDateFormat { get; set; }

    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 48;
    public string TextColor { get; set; } = "#FFFFFF";
    public double TextOpacity { get; set; } = 1.0;
    public double BackgroundOpacity { get; set; } = 0.0;
    public string BackgroundColor { get; set; } = "#000000";

    public double PositionX { get; set; } = 100;
    public double PositionY { get; set; } = 100;

    public bool AlwaysOnTop { get; set; } = true;
    public bool StartWithWindows { get; set; } = false;

    public List<ThemePreset> Presets { get; set; } = new();

    public string GetTimeFormat()
    {
        if (!string.IsNullOrWhiteSpace(CustomTimeFormat))
            return CustomTimeFormat;

        var format = Use24HourFormat ? "HH:mm" : "h:mm tt";
        if (ShowSeconds)
            format = Use24HourFormat ? "HH:mm:ss" : "h:mm:ss tt";
        
        return format;
    }

    public string? GetDateFormat()
    {
        if (!ShowDate) return null;
        return CustomDateFormat ?? "dddd, MMMM d";
    }

    public void ApplyPreset(ThemePreset preset)
    {
        FontFamily = preset.FontFamily;
        FontSize = preset.FontSize;
        TextColor = preset.TextColor;
        TextOpacity = preset.TextOpacity;
        BackgroundOpacity = preset.BackgroundOpacity;
        BackgroundColor = preset.BackgroundColor;
    }

    public ThemePreset ToPreset(string name)
    {
        return new ThemePreset
        {
            Name = name,
            FontFamily = FontFamily,
            FontSize = FontSize,
            TextColor = TextColor,
            TextOpacity = TextOpacity,
            BackgroundOpacity = BackgroundOpacity,
            BackgroundColor = BackgroundColor
        };
    }

    public static ClockSettings CreateDefault() => new();
}
