# Project Structure

## Directory Organization

```
HalloweenMouseMover/
├── Interfaces/          # Interface definitions for dependency injection
├── Models/              # Data models and DTOs
├── Services/            # Core service implementations
├── Utils/               # Utility classes and helpers
├── Program.cs           # Application entry point with DI setup
├── MainForm.cs          # System tray UI and application lifecycle
├── SettingsForm.cs      # Settings dialog UI
└── [Resources]          # Cursor, icon, and audio files
```

## Architecture Patterns

### Dependency Injection
- All services are injected through constructor parameters
- Interfaces defined in `Interfaces/` folder
- Implementations in `Services/` folder
- Services are instantiated in `Program.cs` and wired together

### Service Layer
- **DialogMonitorService** - Polls for new dialog windows using FlaUI
- **ButtonDetector** - Identifies buttons in dialogs and classifies them
- **ButtonClassifier** - Determines which buttons are "negative" actions
- **CursorMover** - Handles smooth cursor animation with trajectories
- **ApplicationController** - Orchestrates all services and handles events
- **ResourceManager** - Manages audio and cursor resources
- **AudioPlayer** - Plays Halloween sound effects
- **CursorManager** - Changes system cursor to Halloween theme

### Event-Driven Design
- `DialogMonitorService` raises `DialogDetected` events
- `ApplicationController` subscribes to events and coordinates responses
- Async/await pattern used for non-blocking cursor animations

## Naming Conventions

- Interfaces: `I` prefix (e.g., `IDialogMonitor`)
- Private fields: `_camelCase` with underscore prefix
- Services: Descriptive names ending in "Service" or functional names (e.g., `DialogMonitorService`, `ButtonDetector`)
- Models: Plain nouns (e.g., `ButtonInfo`, `DialogInfo`)

## Code Style

- Null-safety: Nullable reference types enabled, use `?` annotations
- Exception handling: Try-catch blocks with logging, graceful degradation
- Logging: Debug.WriteLine for development, Console.WriteLine for user-visible messages
- Thread safety: Lock objects used for shared state (e.g., processed dialogs dictionary)
- Disposal: IDisposable pattern for services managing unmanaged resources
