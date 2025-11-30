using System.Drawing;
using FlaUI.Core.AutomationElements;

namespace HalloweenMouseMover.Models
{
    public class ButtonInfo
    {
        public AutomationElement Element { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public Rectangle Bounds { get; set; }
        public bool IsNegative { get; set; }
        public int Priority { get; set; }
    }
}
