# Halloween Mouse Mover

A Windows desktop application that automatically moves the mouse cursor to negative buttons (No, Cancel, Close) when dialog boxes appear, with Halloween-themed sound effects and cursor changes.

## Project Structure

```
HalloweenMouseMover/
├── Interfaces/          # Core interface definitions
│   ├── IApplicationController.cs
│   ├── IAudioPlayer.cs
│   ├── IButtonDetector.cs
│   ├── ICursorManager.cs
│   ├── ICursorMover.cs
│   └── IDialogMonitor.cs
├── Models/              # Data models
│   ├── AppConfiguration.cs
│   ├── ButtonInfo.cs
│   ├── DialogInfo.cs
│   └── DialogProcessingState.cs
├── Services/            # Implementation classes (to be added)
├── Utils/               # Utility classes (to be added)
├── Program.cs           # Application entry point
├── horror_mouse.cur     # Halloween cursor resource
└── Scream03.mp3         # Halloween sound effect
```

## Technology Stack

- **.NET 6** (Windows)
- **Windows Forms** for system tray integration
- **FlaUI** for UI Automation (dialog detection)
- **NAudio** for MP3 audio playback
- **P/Invoke** for cursor manipulation via Win32 API

## Build Requirements

- Visual Studio Community Edition 2022 or later
- .NET 6 SDK
- Windows 10 or later

## Building the Project

```bash
dotnet restore
dotnet build
```

## Next Steps

This is the initial project setup with core interfaces and models. Implementation will follow in subsequent tasks.
