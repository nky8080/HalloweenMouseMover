using System;
using FlaUI.Core.AutomationElements;

namespace HalloweenMouseMover.Models
{
    public class DialogInfo
    {
        public IntPtr WindowHandle { get; set; }
        public AutomationElement DialogElement { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
    }
}
