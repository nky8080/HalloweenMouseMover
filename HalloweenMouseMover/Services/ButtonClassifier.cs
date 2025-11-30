using System;
using System.Collections.Generic;
using System.Linq;
using HalloweenMouseMover.Models;
using HalloweenMouseMover.Utils;

namespace HalloweenMouseMover.Services
{
    public class ButtonClassifier
    {
        private readonly List<string> _negativePatterns;

        public ButtonClassifier()
        {
            // Define negative button text patterns (English and Japanese)
            // Order matters: higher priority patterns should come first
            _negativePatterns = new List<string>
            {
                "No",
                "いいえ",      // No in Japanese (higher priority than Cancel)
                "Cancel",
                "キャンセル",  // Cancel in Japanese
                "Close",
                "閉じる"       // Close in Japanese
            };
        }

        public ButtonInfo? FindNegativeButton(List<ButtonInfo> buttons)
        {
            if (buttons == null || buttons.Count == 0)
            {
                return null;
            }

            // Classify all buttons and calculate priorities
            foreach (var button in buttons)
            {
                ClassifyButton(button);
            }

            // Find the highest priority negative button
            var negativeButtons = buttons.Where(b => b.IsNegative).ToList();
            
            if (negativeButtons.Count == 0)
            {
                return null;
            }

            // Return the button with the highest priority
            return negativeButtons.OrderByDescending(b => b.Priority).First();
        }

        private void ClassifyButton(ButtonInfo button)
        {
            // Check if button text matches any negative pattern
            var matchResult = MatchNegativePattern(button.Text);
            
            if (!matchResult.IsMatch)
            {
                button.IsNegative = false;
                button.Priority = 0;
                return;
            }

            button.IsNegative = true;
            
            // Calculate priority score based on multiple factors
            int priority = 0;

            // Factor 1: Text match confidence (exact match = 100, partial = 50)
            priority += matchResult.IsExactMatch ? 100 : 50;

            // Factor 2: Button type bonus (No/いいえ gets highest priority)
            if (IsNoButton(button.Text))
            {
                priority += 100; // Bonus for "No" buttons (higher than exact match)
            }

            // Factor 3: Position (right side buttons get higher priority)
            // Buttons on the right side typically have higher X coordinates
            priority += CalculatePositionScore(button);

            // Factor 4: Size (larger buttons get slightly higher priority)
            priority += CalculateSizeScore(button);

            button.Priority = priority;
        }

        private bool IsNoButton(string buttonText)
        {
            var noPatterns = new[] { "No", "いいえ", "保存しない" };
            return noPatterns.Any(pattern => 
                buttonText.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0 ||
                buttonText.IndexOf(pattern, StringComparison.Ordinal) >= 0);
        }

        private (bool IsMatch, bool IsExactMatch) MatchNegativePattern(string buttonText)
        {
            if (string.IsNullOrWhiteSpace(buttonText))
            {
                return (false, false);
            }

            foreach (var pattern in _negativePatterns)
            {
                // Check for exact match (case-insensitive for English, case-sensitive for Japanese)
                if (buttonText.Equals(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return (true, true);
                }

                // Also check case-sensitive match for Japanese text
                if (buttonText.Equals(pattern, StringComparison.Ordinal))
                {
                    return (true, true);
                }

                // Check for partial match (pattern contained in button text)
                if (buttonText.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return (true, false);
                }

                // Also check case-sensitive partial match for Japanese
                if (buttonText.IndexOf(pattern, StringComparison.Ordinal) >= 0)
                {
                    return (true, false);
                }
            }

            return (false, false);
        }

        private int CalculatePositionScore(ButtonInfo button)
        {
            // Buttons on the right side get higher scores
            // X position contributes up to 30 points
            // Normalize by dividing by 50 (assuming typical dialog width of 300-600px)
            int positionScore = Math.Min(30, button.Bounds.X / 50);
            return positionScore;
        }

        private int CalculateSizeScore(ButtonInfo button)
        {
            // Larger buttons get slightly higher priority
            // Size contributes up to 20 points
            int area = button.Bounds.Width * button.Bounds.Height;
            int sizeScore = Math.Min(20, area / 500);
            return sizeScore;
        }
    }
}
