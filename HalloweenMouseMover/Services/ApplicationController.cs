using System;
using System.Drawing;
using System.Security;
using System.Threading.Tasks;
using HalloweenMouseMover.Interfaces;
using HalloweenMouseMover.Utils;

namespace HalloweenMouseMover.Services
{
    public class ApplicationController : IApplicationController, IDisposable
    {
        private readonly IDialogMonitor _dialogMonitor;
        private readonly IButtonDetector _buttonDetector;
        private readonly ICursorMover _cursorMover;
        private readonly ResourceManager _resourceManager;
        private bool _isRunning;
        private bool _hasNotifiedSecurityRestriction;
        private readonly object _lockObject = new object();

        public ApplicationController(
            IDialogMonitor dialogMonitor,
            IButtonDetector buttonDetector,
            ICursorMover cursorMover,
            ResourceManager resourceManager)
        {
            _dialogMonitor = dialogMonitor ?? throw new ArgumentNullException(nameof(dialogMonitor));
            _buttonDetector = buttonDetector ?? throw new ArgumentNullException(nameof(buttonDetector));
            _cursorMover = cursorMover ?? throw new ArgumentNullException(nameof(cursorMover));
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));

            // Wire up event handlers
            _dialogMonitor.DialogDetected += OnDialogDetected;
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                {
                    return;
                }

                try
                {
                    // Initialize resources
                    _resourceManager.Initialize();

                    // Start monitoring for dialogs
                    _dialogMonitor.StartMonitoring();

                    _isRunning = true;
                    LogInfo("Application started");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to start application: {ex.Message}", ex);
                    throw;
                }
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                {
                    return;
                }

                try
                {
                    // Stop monitoring
                    _dialogMonitor.StopMonitoring();

                    // Stop any active effects
                    _resourceManager.StopEffects();

                    _isRunning = false;
                    LogInfo("Application stopped");
                }
                catch (Exception ex)
                {
                    LogError($"Error stopping application: {ex.Message}", ex);
                }
            }
        }

        public void ShowSettings()
        {
            try
            {
                LogInfo("Opening settings dialog");
                
                using (var settingsForm = new SettingsForm())
                {
                    var result = settingsForm.ShowDialog();
                    
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        LogInfo("Settings saved successfully");
                    }
                    else
                    {
                        LogInfo("Settings dialog cancelled");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error showing settings dialog: {ex.Message}", ex);
                throw;
            }
        }

        public void Exit()
        {
            try
            {
                Stop();
                LogInfo("Application exiting");
            }
            catch (Exception ex)
            {
                LogError($"Error during exit: {ex.Message}", ex);
            }
        }

        private async void OnDialogDetected(object? sender, DialogDetectedEventArgs e)
        {
            try
            {
                LogInfo($"Dialog detected at {e.DetectedAt}");

                // Detect buttons in the dialog
                var buttons = _buttonDetector.DetectButtons(e.DialogElement);
                LogInfo($"Found {buttons.Count} buttons in dialog");

                if (buttons.Count == 0)
                {
                    LogInfo("No buttons found in dialog");
                    return;
                }

                // Find negative button
                var negativeButton = _buttonDetector.FindNegativeButton(buttons);

                if (negativeButton == null)
                {
                    LogInfo("No negative button found in dialog");
                    return;
                }

                LogInfo($"Found negative button: '{negativeButton.Text}' at ({negativeButton.Bounds.X}, {negativeButton.Bounds.Y})");

                // Calculate target point (center of button)
                Point targetPoint = new Point(
                    negativeButton.Bounds.X + negativeButton.Bounds.Width / 2,
                    negativeButton.Bounds.Y + negativeButton.Bounds.Height / 2
                );

                // Trigger Halloween effects (sound + cursor change)
                _resourceManager.TriggerHalloweenEffects();

                // Move cursor to negative button
                await MoveCursorToButtonAsync(targetPoint);

                LogInfo("Successfully processed dialog");
            }
            catch (UnauthorizedAccessException ex)
            {
                HandleSecurityException("Cursor movement blocked by security restrictions", ex);
            }
            catch (SecurityException ex)
            {
                HandleSecurityException("Security restriction encountered", ex);
            }
            catch (Exception ex)
            {
                LogError($"Error processing dialog: {ex.Message}", ex);
            }
        }

        private async Task MoveCursorToButtonAsync(Point targetPoint)
        {
            try
            {
                // Check if cursor movement is allowed
                if (!_cursorMover.CanMoveCursor())
                {
                    throw new UnauthorizedAccessException("Cursor movement is not allowed");
                }

                // Move cursor with smooth animation
                await _cursorMover.MoveCursorAsync(targetPoint, 200);
                LogInfo($"Cursor moved to ({targetPoint.X}, {targetPoint.Y})");
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw to be handled by caller
            }
            catch (Exception ex)
            {
                LogError($"Error moving cursor: {ex.Message}", ex);
                throw;
            }
        }

        private void HandleSecurityException(string message, Exception ex)
        {
            LogError(message, ex);

            // Display notification only on first occurrence
            if (!_hasNotifiedSecurityRestriction)
            {
                _hasNotifiedSecurityRestriction = true;
                NotifyUser("Security Restriction", 
                    "Halloween Mouse Mover cannot move the cursor due to Windows security restrictions. " +
                    "The application will continue monitoring but cursor movement may be limited.");
            }
        }

        private void LogInfo(string message)
        {
            Logger.Log($"[INFO] {message}");
        }

        private void LogError(string message, Exception? ex = null)
        {
            string errorMessage = ex != null 
                ? $"[ERROR] {message} | Exception: {ex.GetType().Name} - {ex.Message}"
                : $"[ERROR] {message}";
            
            Logger.Log(errorMessage);
        }

        private void NotifyUser(string title, string message)
        {
            // For now, just log the notification
            // In a full implementation, this would show a system tray notification
            LogInfo($"USER NOTIFICATION - {title}: {message}");
            Console.WriteLine($"\n*** {title} ***\n{message}\n");
        }

        public void Dispose()
        {
            try
            {
                Stop();
                
                if (_dialogMonitor is IDisposable disposableMonitor)
                {
                    disposableMonitor.Dispose();
                }
                
                _resourceManager?.Dispose();
            }
            catch (Exception ex)
            {
                LogError($"Error during disposal: {ex.Message}", ex);
            }
        }
    }
}
