using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using HalloweenMouseMover.Interfaces;
using HalloweenMouseMover.Models;
using HalloweenMouseMover.Utils;

namespace HalloweenMouseMover.Services
{
    public class DialogMonitorService : IDialogMonitor, IDisposable
    {
        private readonly UIA3Automation _automation;
        private readonly Dictionary<IntPtr, DialogProcessingState> _processedDialogs;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _monitoringTask;
        private readonly object _lockObject = new object();

        public event EventHandler<DialogDetectedEventArgs>? DialogDetected;

        public int PollingIntervalMs { get; set; }

        public DialogMonitorService(int pollingIntervalMs = 50)
        {
            _automation = new UIA3Automation();
            _processedDialogs = new Dictionary<IntPtr, DialogProcessingState>();
            PollingIntervalMs = pollingIntervalMs;
        }

        public void StartMonitoring()
        {
            if (_monitoringTask != null && !_monitoringTask.IsCompleted)
            {
                return; // Already monitoring
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _monitoringTask = Task.Run(() => MonitoringLoop(_cancellationTokenSource.Token));
        }

        public void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
            _monitoringTask?.Wait(TimeSpan.FromSeconds(2));
        }

        private async Task MonitoringLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    DetectDialogs();
                    CleanupStaleDialogs();
                }
                catch (Exception ex)
                {
                    Logger.Log($"[DialogMonitor] Error in monitoring loop: {ex.Message}");
                }

                await Task.Delay(PollingIntervalMs, cancellationToken);
            }
        }

        private void DetectDialogs()
        {
            try
            {
                var desktop = _automation.GetDesktop();
                var windows = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window));

                foreach (var window in windows)
                {
                    try
                    {
                        var handle = window.Properties.NativeWindowHandle.ValueOrDefault;
                        
                        // Only process new windows (not yet processed)
                        if (handle != IntPtr.Zero && !IsDialogProcessed(handle))
                        {
                            if (IsDialog(window))
                            {
                                OnDialogDetected(window, handle);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"[DialogMonitor] Error checking window: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[DialogMonitor] Error detecting dialogs: {ex.Message}");
            }
        }

        private bool IsDialog(AutomationElement window)
        {
            try
            {
                // Check for dialog patterns
                var className = window.Properties.ClassName.ValueOrDefault ?? string.Empty;
                var windowPattern = window.Patterns.Window.PatternOrDefault;

                // Common dialog class names
                var dialogClassNames = new[]
                {
                    "#32770",  // Standard Windows dialog
                    "Dialog",
                    "MessageBox",
                    "ConfirmDialog"
                };

                if (dialogClassNames.Any(dcn => className.Contains(dcn, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                // Check if it's a modal window
                if (windowPattern != null && windowPattern.IsModal.ValueOrDefault)
                {
                    return true;
                }

                // Check for dialog-like characteristics (has buttons and is small)
                var buttons = window.FindAllChildren(cf => cf.ByControlType(ControlType.Button));
                
                if (buttons.Length > 0)
                {
                    var bounds = window.BoundingRectangle;
                    
                    // Dialogs are typically smaller than full windows
                    if (bounds.Width < 800 && bounds.Height < 600)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[DialogMonitor] Error in IsDialog: {ex.Message}");
            }

            return false;
        }

        private bool IsDialogProcessed(IntPtr handle)
        {
            lock (_lockObject)
            {
                return _processedDialogs.ContainsKey(handle);
            }
        }

        private void OnDialogDetected(AutomationElement dialogElement, IntPtr handle)
        {
            lock (_lockObject)
            {
                // Mark as processing
                _processedDialogs[handle] = new DialogProcessingState
                {
                    WindowHandle = handle,
                    LastProcessedAt = DateTime.Now,
                    IsProcessing = true,
                    Result = ProcessingResult.Success
                };
            }

            // Raise event
            DialogDetected?.Invoke(this, new DialogDetectedEventArgs
            {
                WindowHandle = handle,
                DialogElement = dialogElement,
                DetectedAt = DateTime.Now
            });
        }

        private void CleanupStaleDialogs()
        {
            lock (_lockObject)
            {
                var staleHandles = new List<IntPtr>();

                foreach (var kvp in _processedDialogs)
                {
                    try
                    {
                        // Check if window still exists
                        var element = _automation.FromHandle(kvp.Key);
                        if (element == null || !element.IsAvailable)
                        {
                            staleHandles.Add(kvp.Key);
                        }
                        else
                        {
                            // Remove entries older than 30 seconds
                            if ((DateTime.Now - kvp.Value.LastProcessedAt).TotalSeconds > 30)
                            {
                                staleHandles.Add(kvp.Key);
                            }
                        }
                    }
                    catch
                    {
                        // If we can't access it, consider it stale
                        staleHandles.Add(kvp.Key);
                    }
                }

                foreach (var handle in staleHandles)
                {
                    _processedDialogs.Remove(handle);
                }
            }
        }

        public void Dispose()
        {
            StopMonitoring();
            _cancellationTokenSource?.Dispose();
            _automation?.Dispose();
        }
    }
}
