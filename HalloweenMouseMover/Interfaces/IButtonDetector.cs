using System.Collections.Generic;
using FlaUI.Core.AutomationElements;
using HalloweenMouseMover.Models;

namespace HalloweenMouseMover.Interfaces
{
    public interface IButtonDetector
    {
        List<ButtonInfo> DetectButtons(AutomationElement dialogElement);
        ButtonInfo? FindNegativeButton(List<ButtonInfo> buttons);
    }
}
