using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using HalloweenMouseMover.Interfaces;
using HalloweenMouseMover.Models;

namespace HalloweenMouseMover.Services
{
    public class ButtonDetector : IButtonDetector
    {
        private readonly ButtonClassifier _classifier;

        public ButtonDetector()
        {
            _classifier = new ButtonClassifier();
        }

        public List<ButtonInfo> DetectButtons(AutomationElement dialogElement)
        {
            var buttons = new List<ButtonInfo>();

            try
            {
                // Find all button elements in the dialog
                var buttonElements = dialogElement.FindAllDescendants(cf => 
                    cf.ByControlType(ControlType.Button));

                foreach (var buttonElement in buttonElements)
                {
                    try
                    {
                        // Extract button text
                        string buttonText = GetButtonText(buttonElement);

                        // Get button bounds
                        var boundingRect = buttonElement.BoundingRectangle;
                        var bounds = new Rectangle(
                            (int)boundingRect.X,
                            (int)boundingRect.Y,
                            (int)boundingRect.Width,
                            (int)boundingRect.Height
                        );

                        var buttonInfo = new ButtonInfo
                        {
                            Element = buttonElement,
                            Text = buttonText,
                            Bounds = bounds,
                            IsNegative = false,
                            Priority = 0
                        };

                        buttons.Add(buttonInfo);
                    }
                    catch (Exception)
                    {
                        // Skip buttons that can't be accessed
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if dialog can't be accessed
            }

            return buttons;
        }

        public ButtonInfo? FindNegativeButton(List<ButtonInfo> buttons)
        {
            return _classifier.FindNegativeButton(buttons);
        }

        private string GetButtonText(AutomationElement buttonElement)
        {
            // Try to get text from Name property first
            string text = buttonElement.Name ?? string.Empty;

            // If Name is empty, try to get text from child text elements
            if (string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    var textElements = buttonElement.FindAllDescendants(cf => 
                        cf.ByControlType(ControlType.Text));
                    
                    if (textElements.Length > 0)
                    {
                        text = textElements[0].Name ?? string.Empty;
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            return text.Trim();
        }
    }
}
