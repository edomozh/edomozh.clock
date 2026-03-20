# Edomozh.Clock

A configurable transparent digital clock overlay for Windows.

## Features

- **Transparent overlay** - clock displays over all applications
- **Click-through mode** - mouse events pass through to windows beneath (default)
- **Edit mode** - toggle via tray menu to drag and reposition the clock
- **Configurable always-on-top** - on/off in settings
- **System tray control** - access settings, toggle edit mode, exit
- **Windows autostart** - optional registry-based startup
- **Single instance** - only one clock process allowed

### Time Format Options
- 12/24 hour toggle
- Show/hide seconds
- Show/hide date
- Custom format string

### Appearance Options
- Font family & size
- Text color & opacity
- Background opacity
- Screen position (saved)
- Theme presets (save/load/delete)

## Requirements

- Windows 10/11
- .NET 8.0 Runtime

## Build

```bash
cd src/Edomozh.Clock
dotnet build
```

## Run

```bash
dotnet run --project src/Edomozh.Clock
```

Or run the compiled executable from `bin/Debug/net8.0-windows/`.

## Configuration

Settings are stored in:
```
%APPDATA%\edomozh.clock\settings.config
```

## Usage

1. Run the application - clock overlay appears
2. Right-click system tray icon for menu:
   - **Edit Mode** - toggle to drag/reposition clock
   - **Settings...** - open settings dialog
   - **Exit** - close application
3. Double-click tray icon to open settings

## License

MIT
