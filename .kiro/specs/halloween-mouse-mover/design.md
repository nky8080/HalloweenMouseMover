# Design Document

## Overview

Halloween Mouse Mover is a Windows desktop application built using C# and .NET Framework, targeting Windows 10 and later. The application runs as a background service that monitors for dialog windows and automatically moves the cursor to negative buttons while playing Halloween-themed audio and displaying a horror mouse cursor.

The application leverages Windows UI Automation API for dialog detection and Win32 API for cursor manipulation. It operates with a lightweight footprint and handles modern Windows security restrictions gracefully.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────┐
│                  Main Application                    │
│                  (Windows Forms)                     │
└──────────────┬──────────────────────────────────────┘
               │
               ├──────────────────────────────────────┐
               │                                       │
       ┌───────▼────────┐                    ┌────────▼────────┐
       │ Dialog Monitor │                    │  Resource       │
       │    Service     │                    │  Manager        │
       └───────┬────────┘                    └────────┬────────┘
               │                                       │
       ┌───────▼────────┐                    ┌────────▼────────┐
       │ Button         │                    │ - Audio Player  │
       │ Detector       │                    │ - Cursor        │
       └───────┬────────┘                    │   Manager       │
               │                              └─────────────────┘
       ┌───────▼────────┐
       │ Cursor Mover   │
       └────────────────┘
```

### Component Interaction Flow

1. **Dialog Monitor Service** continuously polls for new dialog windows using UI Automation
2. When a dialog is detected, **Button Detector** analyzes button elements
3. **Button Detector** identifies negative buttons based on text patterns
4. **Cursor Mover** calculates trajectory and moves cursor to target button
5. **Resource Manager** triggers audio playback and cursor change simultaneously
6. After completion, **Resource Manager** restores original cursor

## Components and Interfaces

### 1. Main Application (Entry Point)

**Responsibility**: Application lifecycle management, system tray integration

**Key Classes**:
- `Program`: Application entry point
- `MainForm`: Hidden form for system tray icon and context menu

**Interfaces**:
```csharp
public interface IApplicationController
{
    void Start();
    void Stop();
    void ShowSettings();
    void Exit();
}
```

### 2. Dialog Monitor Service

**Responsibility**: Detect dialog windows using UI Automation API

**Key Classes**:
- `DialogMonitorService`: Background worker that polls for dialogs
- `DialogInfo`: Data structure containing dialog window information

**Interfaces**:
```csharp
public interface IDialogMonitor
{
    event EventHandler<DialogDetectedEventArgs> DialogDetected;
    void StartMonitoring();
    void StopMonitoring();
    int PollingIntervalMs { get; set; }
}

public class DialogDetectedEventArgs : EventArgs
{
    public IntPtr WindowHandle { get; set; }
    public AutomationElement DialogElement { get; set; }
    public DateTime DetectedAt { get; set; }
}
```

**Implementation Details**:
- Uses `System.Windows.Automation` namespace
- Polling interval: 50ms (configurable)
- Filters for windows with dialog class names or patterns
- Caches recently processed dialogs to avoid duplicate processing

### 3. Button Detector

**Responsibility**: Identify and classify buttons within dialog windows

**Key Classes**:
- `ButtonDetector`: Analyzes button elements
- `ButtonInfo`: Data structure for button properties
- `ButtonClassifier`: Determines if button is negative

**Interfaces**:
```csharp
public interface IButtonDetector
{
    List<ButtonInfo> DetectButtons(AutomationElement dialogElement);
    ButtonInfo FindNegativeButton(List<ButtonInfo> buttons);
}

public class ButtonInfo
{
    public AutomationElement Element { get; set; }
    public string Text { get; set; }
    public Rectangle Bounds { get; set; }
    public bool IsNegative { get; set; }
    public int Priority { get; set; }
}
```

**Button Classification Logic**:
- Negative patterns: "No", "Cancel", "Close", "キャンセル", "いいえ", "閉じる"
- Priority scoring based on:
  - Text match confidence (exact > partial)
  - Button position (right side = higher priority)
  - Button size (larger = higher priority)
- Returns highest priority negative button

### 4. Cursor Mover

**Responsibility**: Move cursor smoothly to target button

**Key Classes**:
- `CursorMover`: Handles cursor movement with animation
- `CursorTrajectory`: Calculates movement path

**Interfaces**:
```csharp
public interface ICursorMover
{
    Task MoveCursorAsync(Point targetPoint, int durationMs = 200);
    bool CanMoveCursor();
}
```

**Implementation Details**:
- Uses `user32.dll` `SetCursorPos` for cursor positioning
- Implements smooth movement using easing function (ease-out cubic)
- Movement duration: 200ms
- Samples cursor position at 10ms intervals for smooth animation
- Handles security exceptions when cursor movement is blocked

### 5. Resource Manager

**Responsibility**: Manage audio and cursor resources

**Key Classes**:
- `AudioPlayer`: Plays Halloween sound effects
- `CursorManager`: Changes and restores cursor

**Interfaces**:
```csharp
public interface IAudioPlayer
{
    void PlaySound(string soundFilePath);
    void StopSound();
}

public interface ICursorManager
{
    void SetHorrorCursor(string cursorFilePath);
    void RestoreCursor();
    void RestoreCursorAfterDelay(int delayMs = 2000);
}
```

**Implementation Details**:
- Audio: Uses `System.Media.SoundPlayer` for WAV, `NAudio` library for MP3
- Cursor: Uses `user32.dll` `LoadCursorFromFile` and `SetSystemCursor`
- Resources bundled with application or loaded from application directory
- Fallback behavior when resources are missing

## Data Models

### Configuration Model

```csharp
public class AppConfiguration
{
    public int PollingIntervalMs { get; set; } = 50;
    public int CursorMovementDurationMs { get; set; } = 200;
    public int CursorRestoreDelayMs { get; set; } = 2000;
    public string HalloweenSoundPath { get; set; } = "Scream03.mp3";
    public string HorrorCursorPath { get; set; } = "horror_mouse.cur";
    public bool EnableSound { get; set; } = true;
    public bool EnableCursorChange { get; set; } = true;
    public List<string> NegativeButtonPatterns { get; set; }
}
```

### Dialog Processing State

```csharp
public class DialogProcessingState
{
    public IntPtr WindowHandle { get; set; }
    public DateTime LastProcessedAt { get; set; }
    public bool IsProcessing { get; set; }
    public ProcessingResult Result { get; set; }
}

public enum ProcessingResult
{
    Success,
    NoNegativeButtonFound,
    CursorMovementBlocked,
    DialogClosed,
    Error
}
```

## Error Handling

### Security Restrictions

**Scenario**: Windows security prevents cursor movement or dialog access

**Handling**:
- Catch `UnauthorizedAccessException` and `SecurityException`
- Log the restriction with dialog information
- Display notification to user (first occurrence only)
- Continue monitoring for other dialogs

### Missing Resources

**Scenario**: Audio or cursor files are missing or corrupted

**Handling**:
- Check file existence at startup
- Log warning if resources are missing
- Continue operation with degraded functionality
- Display warning in system tray tooltip

### Dialog Access Failures

**Scenario**: UI Automation cannot access dialog elements

**Handling**:
- Implement retry logic (max 2 attempts)
- Timeout after 500ms
- Log failure and skip dialog
- Continue monitoring

### Unexpected Exceptions

**Scenario**: Unhandled exceptions in background threads

**Handling**:
- Wrap all background operations in try-catch
- Log exception details to file
- Restart affected service component
- Notify user if critical component fails

## Testing Strategy

### Unit Testing

**Framework**: MSTest or NUnit

**Test Coverage**:
- Button classification logic with various text patterns
- Cursor trajectory calculation
- Configuration loading and validation
- Resource path resolution

**Mocking**:
- Mock `AutomationElement` for button detection tests
- Mock file system for resource loading tests

### Integration Testing

**Test Scenarios**:
- End-to-end flow: dialog detection → button identification → cursor movement
- Resource loading and playback
- Configuration changes during runtime

**Test Environment**:
- Windows 10/11 virtual machine
- Test dialogs created using Windows Forms

### Manual Testing

**Test Cases**:
1. Standard Windows dialogs (File Explorer, Notepad save confirmation)
2. Application-specific dialogs (Visual Studio, browsers)
3. Multiple dialogs appearing simultaneously
4. Dialogs with multiple negative buttons
5. Dialogs with no negative buttons
6. Running with restricted permissions

**Validation**:
- Cursor moves smoothly to correct button
- Sound plays at appropriate time
- Cursor changes to horror mouse
- Original cursor restores after delay
- No crashes or hangs

### Performance Testing

**Metrics**:
- Dialog detection latency (target: < 100ms)
- Cursor movement completion time (target: 200ms)
- CPU usage during idle monitoring (target: < 2%)
- Memory footprint (target: < 50MB)

**Tools**:
- Windows Performance Monitor
- Visual Studio Profiler

## Technology Stack

- **Language**: C# 10
- **Framework**: .NET Framework 4.8 or .NET 6
- **IDE**: Visual Studio Community Edition 2022
- **UI Framework**: Windows Forms (for system tray)
- **UI Automation**: System.Windows.Automation
- **Audio**: System.Media.SoundPlayer, NAudio (NuGet package)
- **Win32 Interop**: P/Invoke for cursor manipulation
- **Testing**: MSTest or NUnit
- **Logging**: NLog or Serilog (optional)

## Deployment

**Distribution**:
- Single executable with embedded resources
- Installer optional (NSIS or WiX)
- Portable mode supported

**Requirements**:
- Windows 10 version 1809 or later
- .NET Framework 4.8 or .NET 6 Runtime
- No administrator privileges required (with limitations)

**Installation**:
- Copy executable and resource files to any directory
- Run executable to start monitoring
- Application adds itself to system tray
