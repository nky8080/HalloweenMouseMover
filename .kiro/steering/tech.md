# Technology Stack

## Framework & Runtime

- **.NET 6** (net6.0-windows) - Windows-specific features required
- **Windows Forms** - UI framework for system tray integration and settings dialog
- **C# 10** with nullable reference types enabled

## Key Dependencies

- **FlaUI.UIA3** (v4.0.0) - UI Automation library for dialog detection and button identification
- **NAudio** (v2.2.1) - Audio playback for Halloween sound effects (MP3)
- **System.Windows.Extensions** (v8.0.0) - Extended Windows Forms functionality

## Build System

### Prerequisites
- Visual Studio 2022 or later (Community Edition supported)
- .NET 6 SDK
- Windows 10 or later

### Common Commands

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project HalloweenMouseMover

# Build for release
dotnet build --configuration Release

# Publish self-contained executable
dotnet publish --configuration Release --self-contained true --runtime win-x64
```

### Project Configuration

- Output type: WinExe (Windows application, no console)
- Nullable reference types: Enabled
- Resources (cursor, icon, audio) are copied to output directory

## Platform-Specific APIs

The application uses P/Invoke for low-level cursor manipulation via Win32 API. Security restrictions may apply in certain Windows contexts (UAC-elevated windows, secure desktop).
