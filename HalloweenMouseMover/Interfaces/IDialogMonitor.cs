using System;
using FlaUI.Core.AutomationElements;

namespace HalloweenMouseMover.Interfaces
{
    public class DialogDetectedEventArgs : EventArgs
    {
        public IntPtr WindowHandle { get; set; }
        public AutomationElement DialogElement { get; set; } = null!;
        public DateTime DetectedAt { get; set; }
    }

    public interface IDialogMonitor
    {
        event EventHandler<DialogDetectedEventArgs> DialogDetected;
        void StartMonitoring();
        void StopMonitoring();
        int PollingIntervalMs { get; set; }
    }
}
