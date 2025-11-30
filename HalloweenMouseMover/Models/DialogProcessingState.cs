using System;

namespace HalloweenMouseMover.Models
{
    public enum ProcessingResult
    {
        Success,
        NoNegativeButtonFound,
        CursorMovementBlocked,
        DialogClosed,
        Error
    }

    public class DialogProcessingState
    {
        public IntPtr WindowHandle { get; set; }
        public DateTime LastProcessedAt { get; set; }
        public bool IsProcessing { get; set; }
        public ProcessingResult Result { get; set; }
    }
}
