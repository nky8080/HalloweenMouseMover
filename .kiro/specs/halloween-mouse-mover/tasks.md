# Implementation Plan

- [x] 1. Set up project structure and core interfaces





  - Create a new C# Windows Forms project in Visual Studio Community Edition
  - Add required NuGet packages (NAudio for MP3 support)
  - Add references to System.Windows.Automation
  - Create folder structure: Models, Services, Interfaces, Utils
  - Define all core interfaces (IDialogMonitor, IButtonDetector, ICursorMover, IAudioPlayer, ICursorManager, IApplicationController)
  - _Requirements: 5.1, 5.2, 5.3_

- [x] 2. Implement configuration management





  - Create AppConfiguration model class with default values
  - Implement configuration loading from JSON file (optional, use defaults if missing)
  - Create ConfigurationManager class to handle config access
  - _Requirements: 6.1, 6.2, 6.4_

- [x] 3. Implement resource management components




- [x] 3.1 Create AudioPlayer class


  - Implement IAudioPlayer interface
  - Add support for WAV files using System.Media.SoundPlayer
  - Add support for MP3 files using NAudio library
  - Implement error handling for missing or corrupted audio files
  - Add async playback to avoid blocking
  - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [x] 3.2 Create CursorManager class


  - Implement ICursorManager interface
  - Use P/Invoke to call LoadCursorFromFile and SetSystemCursor from user32.dll
  - Implement SetHorrorCursor method to change cursor
  - Implement RestoreCursor method to restore original cursor
  - Implement RestoreCursorAfterDelay with timer-based restoration
  - Add error handling for missing or corrupted cursor files
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [x] 3.3 Create ResourceManager class


  - Coordinate AudioPlayer and CursorManager
  - Implement simultaneous audio playback and cursor change
  - Handle resource initialization at startup
  - Verify resource files exist and log warnings if missing
  - _Requirements: 3.4, 4.4, 6.3_

- [x] 4. Implement cursor movement functionality





- [x] 4.1 Create CursorMover class


  - Implement ICursorMover interface
  - Use P/Invoke to call SetCursorPos from user32.dll
  - Implement CanMoveCursor method to check for security restrictions
  - _Requirements: 2.1, 2.4, 6.2_

- [x] 4.2 Implement smooth cursor animation


  - Create CursorTrajectory class to calculate movement path
  - Implement ease-out cubic easing function
  - Create MoveCursorAsync method with 200ms duration
  - Sample cursor position at 10ms intervals for smooth movement
  - Add cancellation support if dialog closes during movement
  - _Requirements: 2.2, 2.3_

- [x] 5. Implement button detection and classification




- [x] 5.1 Create ButtonInfo model class


  - Define properties: Element, Text, Bounds, IsNegative, Priority
  - _Requirements: 1.2, 1.3_

- [x] 5.2 Create ButtonDetector class


  - Implement IButtonDetector interface
  - Use UI Automation to find all button elements in dialog
  - Extract button text and bounds from AutomationElement
  - _Requirements: 1.2_

- [x] 5.3 Create ButtonClassifier class


  - Define negative button text patterns (No, Cancel, Close, キャンセル, いいえ, 閉じる)
  - Implement classification logic based on text matching
  - Implement priority scoring based on text match, position, and size
  - Implement FindNegativeButton to return highest priority negative button
  - _Requirements: 1.3, 1.4, 1.5_

- [x] 6. Implement dialog monitoring service




- [x] 6.1 Create DialogInfo and DialogDetectedEventArgs classes


  - Define data structures for dialog information
  - _Requirements: 1.1_

- [x] 6.2 Create DialogMonitorService class


  - Implement IDialogMonitor interface
  - Create background worker thread for polling
  - Use UI Automation to find dialog windows (check for window patterns and class names)
  - Implement 50ms polling interval (configurable)
  - Raise DialogDetected event when new dialog is found
  - _Requirements: 1.1, 6.1_

- [x] 6.3 Implement dialog caching to prevent duplicate processing


  - Create DialogProcessingState class
  - Maintain cache of recently processed dialog handles
  - Check cache before processing new dialogs
  - Remove stale entries from cache (dialogs that no longer exist)
  - _Requirements: 1.1_

- [x] 7. Implement main application controller




- [x] 7.1 Create ApplicationController class


  - Implement IApplicationController interface
  - Wire up DialogMonitorService with event handlers
  - Implement event handler for DialogDetected event
  - Coordinate ButtonDetector, CursorMover, and ResourceManager
  - Implement complete flow: detect dialog → find negative button → move cursor + play sound + change cursor
  - _Requirements: 1.1, 1.2, 1.3, 2.1, 2.2, 3.1, 4.1_

- [x] 7.2 Add error handling and logging


  - Wrap all operations in try-catch blocks
  - Handle UnauthorizedAccessException and SecurityException for cursor movement
  - Handle exceptions from UI Automation access
  - Log errors to file or debug output
  - Display user notification on first security restriction encounter
  - _Requirements: 2.4, 3.4, 4.4, 6.1, 6.2, 6.3_

- [x] 8. Create main form and system tray integration




- [x] 8.1 Create MainForm (hidden Windows Form)


  - Add NotifyIcon component for system tray
  - Set application icon
  - Create context menu with Start, Stop, Settings, Exit options
  - _Requirements: 5.1, 5.2_

- [x] 8.2 Implement Program entry point


  - Create Main method
  - Initialize ApplicationController
  - Show MainForm (hidden)
  - Start dialog monitoring
  - Run application message loop
  - _Requirements: 5.1, 5.2_

- [x] 9. Add resource files to project





  - Add horror_mouse.cur file to project (already exists in workspace)
  - Add Scream03.mp3 file to project (already exists in workspace)
  - Set files to "Copy to Output Directory"
  - Update ResourceManager to load from application directory
  - _Requirements: 3.2, 3.3, 4.2, 4.3_

- [x] 10. Implement settings dialog (optional feature)





  - Create SettingsForm with configuration options
  - Add checkboxes for EnableSound and EnableCursorChange
  - Add numeric inputs for timing parameters
  - Save configuration changes to JSON file
  - _Requirements: 6.3_

- [ ]* 11. Create unit tests for core logic
  - Write tests for ButtonClassifier with various text patterns
  - Write tests for CursorTrajectory calculation
  - Write tests for configuration loading
  - Mock AutomationElement for button detection tests
  - _Requirements: 1.3, 1.4, 2.3_

- [ ]* 12. Perform integration testing
  - Test end-to-end flow with test dialogs
  - Test with standard Windows dialogs (Notepad, File Explorer)
  - Test with multiple simultaneous dialogs
  - Test with dialogs containing no negative buttons
  - Verify audio playback timing
  - Verify cursor change and restoration
  - _Requirements: 1.1, 1.2, 1.3, 2.1, 2.2, 3.1, 4.1, 4.2_

- [ ]* 13. Add documentation
  - Create README.md with project description and usage instructions
  - Document known limitations due to Windows security
  - Add code comments for complex logic
  - Create user guide for hackathon submission
  - _Requirements: 6.4_
