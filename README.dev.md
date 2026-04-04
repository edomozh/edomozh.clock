## Build

```bash
dotnet build
```

## Publish (Single EXE)

```bash
dotnet publish
```

Output: `edomozh.clock/bin/Release/net8.0-windows/win-x64/publish/edomozh.clock.exe`

This produces a single self-contained executable that includes the .NET runtime.

## Run

```bash
dotnet run --project edomozh.clock
```

Or run the compiled executable from `bin/Debug/net8.0-windows/`.

## Configuration

Settings are stored in:
```
%APPDATA%\edomozh.clock\settings.config
```

## License

MIT
